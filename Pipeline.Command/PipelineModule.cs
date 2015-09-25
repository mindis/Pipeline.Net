using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Reader;
using Pipeline.Logging;
using Pipeline.Configuration;
using Pipeline.Desktop;
using Pipeline.Desktop.Actions;
using Pipeline.Desktop.Transformers;
using Pipeline.Transformers;
using Pipeline.Provider.SqlServer;
using Pipeline.Validators;
using Pipeline.Transformers.System;
using Pipeline.Interfaces;
using Pipeline.Template.Razor;
using Action = Pipeline.Configuration.Action;

namespace Pipeline.Command {

    public class PipelineModule : Module {

        const string SqlMinDateTransform = "sys.sqlmindate";
        readonly LogLevel _level;
        readonly Root _root;

        public PipelineModule(
            Root root,
            LogLevel level = LogLevel.Info
        ) {
            _root = root;
            _level = level;
        }

        protected override void Load(ContainerBuilder builder) {

            builder.Register<IPipelineLogger>(ctx => new ConsoleLogger(_level)).SingleInstance();

            foreach (var process in _root.Processes) {
                RegisterMaps(builder, process);
                RegisterTemplates(builder, process);
                RegisterActions(builder, process);
                RegisterCalculatedFieldTransforms(builder, process);
                RegisterEntities(builder, process);
                RegisterProcess(builder, process);
            }

        }

        static void RegisterProcess(ContainerBuilder builder, Process process) {
            builder.Register<IProcessController>(ctx => {

                var pipelines = new List<IEntityPipeline>();
                var deleteHandlers = new List<IEntityDeleteHandler>();
                foreach (var entity in process.Entities) {
                    var pipeline = ctx.ResolveNamed<IEntityPipeline>(entity.Key);
                    pipelines.Add(ResolveEntityPipeline(ctx, pipeline, process, entity));
                    if (entity.Delete) {
                        deleteHandlers.Add(ctx.ResolveNamed<IEntityDeleteHandler>(entity.Key));
                    }
                }

                var outputProvider = process.Connections.First(c => c.Name == "output").Provider;
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process);

                var controller = new ProcessController(pipelines, deleteHandlers);

                if (process.Mode == "init") {
                    switch (outputProvider) {
                        case "sqlserver":
                            var output = new OutputContext(context, new Incrementer(context));
                            controller.PreActions.Add(new SqlInitializer(output));
                            controller.PostActions.Add(new SqlStarViewCreator(output));
                            break;
                    }
                }

                foreach (var template in process.Templates.Where(t => t.Enabled)) {
                    controller.PreActions.Add(new RenderTemplateAction(template, ctx.ResolveNamed<ITemplateEngine>(template.Name)));
                    foreach (var action in template.Actions.Where(a => a.GetModes().Any(m => m == process.Mode))) {
                        if (action.Before) {
                            controller.PreActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                        }
                        if (action.After) {
                            controller.PostActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                        }
                    }
                }

                foreach (var action in process.Actions.Where(a => a.GetModes().Any(m => m == process.Mode))) {
                    if (action.Before) {
                        controller.PreActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                    if (action.After) {
                        controller.PostActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                }

                return controller;
            }).Named<IProcessController>(process.Key);
        }

        static IEntityPipeline ResolveEntityPipeline(
            IComponentContext ctx,
            IEntityPipeline pipeline,
            Process process,
            Entity entity
        ) {
            var outputProvider = process.Connections.First(c => c.Name == "output").Provider;

            // extract
            pipeline.Register(ctx.ResolveNamed<IReadInput>(entity.Key));

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
            pipeline.Register(ctx.ResolveNamed<IWriteOutput>(entity.Key));
            pipeline.Register(ctx.ResolveNamed<IUpdate>(entity.Key));
            return pipeline;
        }

        static void RegisterEntities(ContainerBuilder builder, Process process) {
            foreach (var e in process.Entities) {

                var entity = e;

                //controller
                builder.Register(ctx => {
                    var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var output = new OutputContext(context, new Incrementer(context));

                    IEntityController controller = new NullEntityController();

                    switch (provider) {
                        case "sqlserver":
                            context.Debug("Registering sql server controller");
                            var initializer = process.Mode == "init" ? (IAction)new SqlEntityInitializer(output) : new NullInitializer();
                            controller = new SqlEntityController(output, initializer);
                            break;
                        default:
                            context.Debug("Registering null controller");
                            break;
                    }

                    return controller;
                }).Named<IEntityController>(entity.Key);

                var type = process.Pipeline == "defer" ? entity.Pipeline : process.Pipeline;

                builder.Register<IEntityPipeline>((ctx) => {
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var pipeline = new DefaultPipeline(ctx.ResolveNamed<IEntityController>(entity.Key), context);
                    switch (type) {
                        case "parallel.linq":
                            context.Debug("Registering {0} pipeline.", type);
                            return new ParallelPipeline(pipeline);
                        default:
                            context.Debug("Registering linq pipeline.", type);
                            return pipeline;
                    }
                }).Named<IEntityPipeline>(entity.Key);

                //input
                builder.Register<IReadInput>((ctx) => {
                    var connection = process.Connections.First(cn => cn.Name == entity.Connection);
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var entityInput = new InputContext(context, new Incrementer(context));
                    switch (connection.Provider) {
                        case "internal":
                            context.Debug("Registering {0} provider", connection.Provider);
                            return new DataSetEntityReader(entityInput);
                        case "sqlserver":
                            if (entityInput.Entity.ReadSize == 0) {
                                context.Debug("Registering {0} reader", connection.Provider);
                                return new SqlInputReader(entityInput, entityInput.InputFields);
                            }
                            context.Debug("Registering {0} batch reader", connection.Provider);
                            return new SqlInputBatchReader(
                                entityInput,
                                new SqlInputReader(entityInput, entityInput.Entity.GetPrimaryKey())
                                );
                        default:
                            context.Warn("Registering null reader", connection.Provider);
                            return new NullEntityReader();
                    }
                }).Named<IReadInput>(entity.Key);

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
                builder.Register<IWriteOutput>((ctx) => {
                    var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var incrementer = new Incrementer(context);
                    var entityOutput = new OutputContext(context, incrementer);
                    switch (provider) {
                        case "sqlserver":
                            context.Debug("Registering {0} writer", provider);
                            return new SqlEntityBulkInserter(entityOutput);
                        default:
                            context.Warn("Registering null writer", provider);
                            return new NullEntityWriter();
                    }
                }).Named<IWriteOutput>(entity.Key);

                //master updater
                builder.Register<IUpdate>((ctx) => {
                    var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                    var incrementer = new Incrementer(context);
                    var entityOutput = new OutputContext(context, incrementer);
                    switch (provider) {
                        case "sqlserver":
                            context.Debug("Registering {0} master updater", provider);
                            return new SqlMasterUpdater(entityOutput);
                        default:
                            context.Warn("Registering null updater");
                            return new NullMasterUpdater();
                    }
                }).Named<IUpdate>(entity.Key);

                //register the delete handler
                if (entity.Delete) {
                    builder.Register<IEntityDeleteHandler>(ctx => {
                        var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);

                        var inputConnection = process.Connections.First(c => c.Name == e.Connection);

                        IRead input = new NullReader();
                        switch (inputConnection.Provider) {
                            case "sqlserver":
                                input = new SqlReader(context, entity.GetPrimaryKey(), ReadFrom.Input);
                                break;
                        }

                        IRead output = new NullReader();
                        IDelete deleter = new NullDeleter();
                        var outputConnection = process.Connections.First(c => c.Name == "output");
                        switch (outputConnection.Provider) {
                            case "sqlserver":
                                output = new SqlReader(context, entity.GetPrimaryKey(), ReadFrom.Output);
                                deleter = new SqlDeleter(new OutputContext(context, new Incrementer(context)));
                                break;
                        }

                        return new ParallelDeleteHandler(new DefaultDeleteHandler(entity, input, output, deleter));
                    }).Named<IEntityDeleteHandler>(entity.Key);
                }

            }
        }

        static void RegisterSqlMinDateTransform(ContainerBuilder builder, Process process, Entity entity) {
            if (process.Connections.First(c => c.Name == "output").Provider == "sqlserver") {
                builder.Register<ITransform>(ctx => {
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

        static void RegisterActions(ContainerBuilder builder, Process process) {
            foreach (var action in process.Templates.Where(t => t.Enabled).SelectMany(t => t.Actions).Where(a => a.GetModes().Any(m => m == process.Mode))) {
                builder.Register(ctx => SwitchAction(ctx, process, action)).Named<IAction>(action.Key);
            }
            foreach (var action in process.Actions.Where(a => a.GetModes().Any(m => m == process.Mode))) {
                builder.Register(ctx => SwitchAction(ctx, process, action)).Named<IAction>(action.Key);
            }
        }

        private static IAction SwitchAction(IComponentContext ctx, Process process, Action action) {
            var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process);
            switch (action.Name) {
                case "copy":
                    return action.InTemplate ? (IAction)
                        new ContentToFileAction(context, action) :
                        new FileToFileAction(context, action);
                case "web":
                    return new WebAction(context, action);
                default:
                    context.Error("{0} action is not registered.", action.Name);
                    return new NullAction();
            }
        }

        static void RegisterTemplates(ContainerBuilder builder, Process process) {

            // Using Cfg-Net.Reader to read templates, for now.
            if (process.Templates.Any()) {
                builder.RegisterType<SourceDetector>().As<ISourceDetector>();
                builder.RegisterType<FileReader>().Named<IReader>("file");
                builder.RegisterType<WebReader>().Named<IReader>("web");

                builder.Register<IReader>(ctx => new DefaultReader(
                    ctx.Resolve<ISourceDetector>(),
                    ctx.ResolveNamed<IReader>("file"),
                    ctx.ResolveNamed<IReader>("web")
                ));
            }

            foreach (var t in process.Templates.Where(t => t.Enabled)) {
                var template = t;
                builder.Register<ITemplateEngine>(ctx => {
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process);
                    switch (template.Engine) {
                        case "razor":
                            return new RazorTemplateEngine(context, template, ctx.Resolve<IReader>());
                        default:
                            return new NullTemplateEngine();
                    }
                }).Named<ITemplateEngine>(t.Name);
            }
        }

        static void RegisterMaps(ContainerBuilder builder, Process process) {
            foreach (var m in process.Maps) {
                var map = m;
                builder.Register<IMapReader>(ctx => {
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
                case "timezone": return new TimeZoneOperation(context);
                case "trim": return new TrimTransform(context);
                case "trimstart": return new TrimStartTransform(context);
                case "trimend": return new TrimEndTransform(context);
                case "javascript": return new JintTransform(context);
                case "tostring": return new ToStringTransform(context);
                case "toupper": return new ToUpperTransform(context);
                case "tolower": return new ToLowerTransform(context);
                case "join": return new JoinTransform(context);
                case "map": return new MapTransform(context, ctx.ResolveNamed<IMapReader>(context.Transform.Map));
                case "decompress": return new DecompressTransform(context);

                case "contains": return new ContainsValidater(context);
                case "is": return new IsValidator(context);

                default:
                    context.Warn("The {0} method is undefined.", context.Transform.Method);
                    return new NullTransformer(context);
            }
        }
    }
}