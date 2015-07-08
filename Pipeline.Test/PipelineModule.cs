using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Linq;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;
using Pipeline.Transformers;
using Pipeline.Validators;

namespace Pipeline.Test {

    public class PipelineModule : Module {

        private readonly Root _root;

        public PipelineModule(Root root) {
            _root = root;
        }

        protected override void Load(ContainerBuilder builder) {

            builder.Register<IPipelineLogger>((ctx) => new DebugLogger());

            foreach (var p in _root.Processes) {

                var process = p;

                foreach (var e in process.Entities) {

                    var entity = e;

                    var pipeline = process.Pipeline == "defer" ? entity.Pipeline : process.Pipeline;

                    builder.Register<IPipeline>((ctx) => {
                        switch (pipeline) {
                            case "linq":
                                return new DefaultPipeline(ctx.Resolve<IPipelineLogger>());
                            case "parallel.linq":
                                return new Parallel(ctx.Resolve<IPipelineLogger>());
                            case "streams":
                                return new Streams.Serial(ctx.Resolve<IPipelineLogger>());
                            case "parallel.streams":
                                return new Streams.Parallel(ctx.Resolve<IPipelineLogger>());
                            case "linq.optimizer":
                                return new Linq.Optimizer.Serial(ctx.Resolve<IPipelineLogger>());
                            case "parallel.linq.optimizer":
                                return new Linq.Optimizer.Parallel(ctx.Resolve<IPipelineLogger>());
                            default:
                                return new DefaultPipeline(ctx.Resolve<IPipelineLogger>());
                        }
                    }).Named<IPipeline>(entity.Key);

                    builder.Register<IEntityReader>((c) => {
                        var connection = process.Connections.First(cn => cn.Name == entity.Connection);
                        switch (connection.Provider) {
                            case "internal":
                                return new DataSetEntityReader(process, entity, c.Resolve<IPipelineLogger>());
                            case "sqlserver":
                                return new SqlEntityReader(process, entity, c.Resolve<IPipelineLogger>());
                        }
                        return null;
                    }).Named<IEntityReader>(entity.Key);

                    foreach (var f in entity.GetAllFields().Where(f => f.Transforms.Any())) {

                        var field = f;

                        if (field.RequiresCompositeValidator()) {
                            builder.Register<ITransform>((ctx) => new CompositeValidator(
                                process,
                                entity,
                                field,
                                field.Transforms.Select(t => SwitchTransform(process, entity, field, t, ctx.Resolve<IPipelineLogger>())),
                                ctx.Resolve<IPipelineLogger>()
                            )).Named<ITransform>(field.Key);
                        } else {
                            foreach (var t in field.Transforms) {
                                var transform = t;
                                builder.Register((ctx) => SwitchTransform(process, entity, field, transform, ctx.Resolve<IPipelineLogger>())).Named<ITransform>(t.Key);
                            }
                        }
                    }
                }

                builder.Register((ctx) => {
                    var pipelines = new List<IPipeline>();
                    foreach (var entity in process.Entities) {
                        var pipeline = ctx.ResolveNamed<IPipeline>(entity.Key);
                        pipeline.Input(ctx.ResolveNamed<IEntityReader>(entity.Key).Read());
                        pipeline.Register(new DefaultNulls(process, entity, null, entity.GetDefaultOf<Transform>(t=>{ t.Method = "defaultnulls";}), ctx.Resolve<IPipelineLogger>()));
                        foreach (var field in entity.GetAllFields().Where(f => f.Transforms.Any())) {
                            if (field.RequiresCompositeValidator()) {
                                pipeline.Register(ctx.ResolveNamed<ITransform>(field.Key));
                            } else {
                                foreach (var transform in field.Transforms) {
                                    pipeline.Register(ctx.ResolveNamed<ITransform>(transform.Key));
                                }
                            }

                        }
                        pipelines.Add(pipeline);
                    }
                    return pipelines;
                }).Named<IEnumerable<IPipeline>>(process.Key);

            }

        }

        private static ITransform SwitchTransform(Process process, Entity entity, Field field, Pipeline.Configuration.Transform transform, IPipelineLogger logger) {
            switch (transform.Method) {
                case "format":
                    return new FormatTransform(process, entity, field, transform, logger);
                case "left":
                    return new LeftTransform(process, entity, field, transform, logger);
                case "right":
                    return new RightTransform(process, entity, field, transform, logger);
                case "copy":
                    return new CopyTransform(process, entity, field, transform, logger);
                case "concat":
                    return new ConcatTransform(process, entity, field, transform, logger);
                case "fromxml":
                    return new FromXmlTransform(process, entity, field, transform, logger);
                case "htmldecode":
                    return new HtmlDecodeTransform(process, entity, field, transform, logger);
                case "xmldecode":
                    return new XmlDecodeTransform(process, entity, field, transform, logger);
                case "hashcode":
                    return new HashcodeTransform(process, entity, field, transform, logger);
                case "contains":
                    return new ContainsValidate(process, entity, field, transform, logger);
                case "padleft":
                    return new PadLeftTransform(process, entity, field, transform, logger);
                case "padright":
                    return new PadRightTransform(process, entity, field, transform, logger);
            }
            return new NullTransformer(process, entity, field, transform, logger);
        }
    }

    public class DefaultNulls : BaseTransform, ITransform {
        private readonly Field[] _fields;
        private readonly Dictionary<string, object> _typeDefaults;
        private readonly Dictionary<int, Func<object>> _getDefaultFor = new Dictionary<int, Func<object>>();

        public DefaultNulls(Process process, Entity entity, Field field, Pipeline.Configuration.Transform transform, IPipelineLogger logger)
            : base(process, entity, field, transform, logger) {
            _fields = entity.GetAllFields().ToArray();
            _typeDefaults = Constants.TypeDefaults();
            foreach (var fld in _fields) {
                var f = fld;
                _getDefaultFor[f.Index] = () => f.Default == Constants.DefaultSetting ? _typeDefaults[f.Type] : f.Convert(f.Default);
            }
        }

        public Row Transform(Row row) {
            foreach (var field in _fields.Where(field => row[field] == null)) {
                row[field] = _getDefaultFor[field.Index]();
            }
            Increment();
            return row;
        }

        public Pipeline.Configuration.Transform InterpretShorthand(string args, List<string> problems) {
            throw new System.NotImplementedException();
        }
    }
}