﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Linq;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;
using Pipeline.Transformers;
using Pipeline.Validators;
using Pipeline.Transformers.System;

namespace Pipeline.Test {

    public class PipelineModule : Module {

        const string SqlMinDateTransform = "sys.sqlmindate";
        const string TruncateTransform = "sys.truncate";
        readonly LogLevel _level;

        public Root Root { get; set; }

        public PipelineModule(string cfg, string shorthand, LogLevel level = LogLevel.Info) {
            _level = level;
            Root = new Root(cfg, shorthand, new JintParser());
        }

        protected override void Load(ContainerBuilder builder) {

            builder.Register<IPipelineLogger>((ctx) => new TraceLogger(_level)).SingleInstance();

            foreach (var process in Root.Processes) {
                RegisterMaps(builder, process);
                RegisterCalculatedFieldTransforms(builder, process);
                RegisterEntities(builder, process);
                RegisterProcess(builder, process);
            }

        }

        static void RegisterProcess(ContainerBuilder builder, Process process) {
            builder.Register((ctx) => {
                var pipelines = new List<IEntityPipeline>();
                foreach (var entity in process.Entities) {
                    var pipeline = ctx.ResolveNamed<IEntityPipeline>(entity.Key);
                    pipelines.Add(ResolveEntityPipeline(ctx, pipeline, process, entity));
                }
                return pipelines;
            }).Named<IEnumerable<IEntityPipeline>>(process.Key);
        }

        static IEntityPipeline ResolveEntityPipeline(
            IComponentContext ctx,
            IEntityPipeline pipeline,
            Process process,
            Entity entity
        ) {
            var outputProvider = process.Connections.First(c => c.Name == "output").Provider;

            // extract
            pipeline.Register(ctx.ResolveNamed<IRead>(entity.Key));

            // configured transforms
            foreach (var field in entity.GetAllFields().Where(f => f.Transforms.Any())) {
                if (field.RequiresCompositeValidator()) {
                    pipeline.Register(ctx.ResolveNamed<ITransform>(field.Key));
                } else {
                    pipeline.Register(field.Transforms.Select(t => ctx.ResolveNamed<ITransform>(t.Key)));
                }
            }

            // output specific transforms
            if (outputProvider == "sqlserver")
                pipeline.Register(ctx.ResolveNamed<ITransform>(entity.Key + SqlMinDateTransform));

            //load
            pipeline.Register(ctx.ResolveNamed<IWrite>(entity.Key));
            pipeline.Register(ctx.ResolveNamed<IUpdate>(entity.Key));
            return pipeline;
        }

        static void RegisterEntities(ContainerBuilder builder, Process process) {
            foreach (var e in process.Entities) {

                var entity = e;
                var entityContext = new PipelineContext(new NullLogger(), process, entity);

                //controller
                builder.Register<IEntityController>((ctx) => {
                    var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var output = new OutputContext(context, new Incrementer(context));
                    switch (provider) {
                        case "sqlserver":
                            return new SqlEntityController(output, new SqlEntityInitializer(output));
                        default:
                            return new NullEntityController();
                    }
                }).Named<IEntityController>(entity.Key);

                var type = process.Pipeline == "defer" ? entity.Pipeline : process.Pipeline;

                builder.Register<IEntityPipeline>((ctx) => {
                    var pipeline = new DefaultPipeline(ctx.ResolveNamed<IEntityController>(entity.Key), entityContext);
                    switch (type) {
                        case "parallel.linq":
                            return new Parallel(pipeline);
                        case "streams":
                            return new Streams.Serial(pipeline);
                        case "parallel.streams":
                            return new Streams.Parallel(pipeline);
                        case "linq.optimizer":
                            return new Linq.Optimizer.Serial(pipeline);
                        case "parallel.linq.optimizer":
                            return new Linq.Optimizer.Parallel(pipeline);
                        default:
                            return pipeline;
                    }
                }).Named<IEntityPipeline>(entity.Key);

                //input
                builder.Register<IRead>((ctx) => {
                    var connection = process.Connections.First(cn => cn.Name == entity.Connection);
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var entityInput = new InputContext(context, new Incrementer(context));
                    switch (connection.Provider) {
                        case "internal":
                            return new DataSetEntityReader(entityInput);
                        case "sqlserver":
                            if(entityInput.Connection.BatchSize == 0) {
                                return new SqlEntityReader(entityInput, entityInput.InputFields);
                            }
                            return new SqlEntityBatchReader(
                                entityInput, 
                                new SqlEntityReader(entityInput,
                                    entityInput.Entity.GetPrimaryKey()
                                )
                            );
                        default:
                            return new NullEntityReader();
                    }
                }).Named<IRead>(entity.Key);

                foreach (var f in entity.GetAllFields().Where(f => f.Transforms.Any())) {

                    var field = f;

                    if (field.RequiresCompositeValidator()) {
                        builder.Register<ITransform>((ctx) => new CompositeValidator(
                            new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field),
                            field.Transforms.Select(t => SwitchTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t)))
                        )).Named<ITransform>(field.Key);
                    } else {
                        foreach (var t in field.Transforms) {
                            var transform = t;
                            builder.Register((ctx) => SwitchTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, transform))).Named<ITransform>(transform.Key);
                        }
                    }
                }

                RegisterSqlMinDateTransform(builder, process, entity);

                //output
                builder.Register<IWrite>((ctx) => {
                    var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var incrementer = new Incrementer(context);
                    var entityOutput = new OutputContext(context, incrementer);
                    switch (provider) {
                        case "sqlserver":
                            return new SqlEntityBulkInserter(entityOutput);
                        default:
                            return new NullEntityWriter();
                    }
                }).Named<IWrite>(entity.Key);

                //master updater
                builder.Register<IUpdate>((ctx) => {
                    var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var incrementer = new Incrementer(context);
                    var entityOutput = new OutputContext(context, incrementer);
                    switch (provider) {
                        case "sqlserver":
                            return new SqlMasterUpdater(entityOutput);
                        default:
                            return new NullMasterUpdater();
                    }
                }).Named<IUpdate>(entity.Key);

            }
        }

        static void RegisterSqlMinDateTransform(ContainerBuilder builder, Process process, Entity entity) {
            if (process.Connections.First(c => c.Name == "output").Provider == "sqlserver") {
                builder.Register<ITransform>((ctx) => {
                    var sqlDatesContext = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, null, entity.GetDefaultOf<Transform>(t => { t.Method = SqlMinDateTransform; }));
                    return new MinDateTransform(sqlDatesContext, new DateTime(1753, 1, 1));
                }).Named<ITransform>(entity.Key + SqlMinDateTransform);
            }
        }

        static void RegisterCalculatedFieldTransforms(ContainerBuilder builder, Process process) {
            foreach (var f in process.CalculatedFields.Where(f => f.Transforms.Any())) {
                var field = f;
                if (field.RequiresCompositeValidator()) {
                    builder.Register<ITransform>((ctx) => new CompositeValidator(
                        new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, null, field),
                        field.Transforms.Select(t => SwitchTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, null, field, t)))
                    )).Named<ITransform>(field.Key);
                } else {
                    foreach (var t in field.Transforms) {
                        var transform = t;
                        builder.Register((ctx) => SwitchTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, null, field, transform))).Named<ITransform>(transform.Key);
                    }
                }
            }
        }

        static void RegisterMaps(ContainerBuilder builder, Process process) {
            foreach (var m in process.Maps) {
                var map = m;
                builder.Register<IMapReader>((ctx) => {
                    var connection = process.Connections.FirstOrDefault(cn => cn.Name == map.Connection);
                    var provider = connection == null ? string.Empty : connection.Provider;
                    switch (provider) {
                        case "sqlserver":
                            return new SqlMapReader();
                        default:
                            return new DefaultMapReader();
                    }
                }).Named<IMapReader>(map.Name);
            }
        }

        static ITransform SwitchTransform(IComponentContext ctx, PipelineContext context) {
            switch (context.Transform.Method) {
                case "format": return new FormatTransform(context);
                case "left": return new LeftTransform(context);
                case "right": return new RightTransform(context);
                case "copy": return new CopyTransform(context);
                case "concat": return new ConcatTransform(context);
                case "fromxml": return new FromXmlTransform(context);
                case "fromsplit": return new FromSplitTransform(context);
                case "htmldecode": return new HtmlDecodeTransform(context);
                case "xmldecode": return new HtmlDecodeTransform(context);
                case "hashcode": return new HashcodeTransform(context);
                case "padleft": return new PadLeftTransform(context);
                case "padright": return new PadRightTransform(context);
                case "splitlength": return new SplitLengthTransform(context);
                case "trim": return new TrimTransform(context);
                case "trimstart": return new TrimStartTransform(context);
                case "trimend": return new TrimEndTransform(context);
                case "javascript": return new JintTransform(context);
                case "tostring": return new ToStringTransform(context);
                case "toupper": return new ToUpperTransform(context);
                case "tolower": return new ToLowerTransform(context);
                case "join": return new JoinTransform(context);
                case "map": return new MapTransform(context, ctx.ResolveNamed<IMapReader>(context.Transform.Map));

                case "contains": return new ContainsValidater(context);
                case "is": return new IsValidator(context);

                default: return new NullTransformer(context);
            }
        }
    }
}