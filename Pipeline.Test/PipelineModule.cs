using System.Collections.Generic;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Linq;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;
using Pipeline.Shorthand;
using Pipeline.Transformers;
using Pipeline.Validators;

namespace Pipeline.Test {

    public class PipelineModule : Module {

        public string[] Warnings { get; set; }
        public string[] Errors { get; set; }
        public Root Root { get; set; }

        public PipelineModule(string cfg) {

            Field.ShorthandParsers["format"] = FormatTransform.InterpretShorthand;

            Field.ShorthandParsers["left"] = LeftTransform.InterpretShorthand;
            Field.ShorthandParsers["right"] = RightTransform.InterpretShorthand;
            Field.ShorthandParsers["copy"] = CopyTransform.InterpretShorthand;
            Field.ShorthandParsers["concat"] = new ParameterlessParser("concat").Parse;
            Field.ShorthandParsers["fromxml"] = FromXmlTransform.InterpretShorthand;
            Field.ShorthandParsers["htmldecode"] = new ParameterlessParser("concat").Parse;
            Field.ShorthandParsers["xmldecode"] = new ParameterlessParser("concat").Parse;
            Field.ShorthandParsers["hashcode"] = new ParameterlessParser("concat").Parse;
            Field.ShorthandParsers["padleft"] = PadLeftTransform.InterpretShorthand;
            Field.ShorthandParsers["padright"] = PadRightTransform.InterpretShorthand;
            Field.ShorthandParsers["splitlength"] = SplitLengthTransform.InterpretShorthand;

            Field.ShorthandParsers["contains"] = ContainsValidater.InterpretShorthand;
            Field.ShorthandParsers["is"] = IsValidator.InterpretShorthand;
            Root = new Root(cfg);
        }

        protected override void Load(ContainerBuilder builder) {

            builder.Register<IPipelineLogger>((ctx) => new DebugLogger()).SingleInstance();

            foreach (var p in Root.Processes) {

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

                    builder.Register<IEntityReader>((ctx) => {
                        var connection = process.Connections.First(cn => cn.Name == entity.Connection);
                        var context = new PipelineContext(process, entity);
                        switch (connection.Provider) {
                            case "internal":
                                return new DataSetEntityReader(context);
                            case "sqlserver":
                                return new SqlEntityReader(context);
                        }
                        return null;
                    }).Named<IEntityReader>(entity.Key);

                    foreach (var f in entity.GetAllFields().Where(f => f.Transforms.Any())) {

                        var field = f;

                        if (field.RequiresCompositeValidator()) {
                            var transforms = field.Transforms.Select(t => SwitchTransform(new PipelineContext(process, entity, field, t)));
                            builder.Register<ITransform>((ctx) => new CompositeValidator(
                                new PipelineContext(process, entity, field, null),
                                transforms
                            )).Named<ITransform>(field.Key);
                        } else {
                            foreach (var t in field.Transforms) {
                                var context = new PipelineContext(process, entity, field, t);
                                builder.Register((ctx) => SwitchTransform(context)).Named<ITransform>(t.Key);
                            }
                        }
                    }
                }

                builder.Register((ctx) => {

                    var pipelines = new List<IPipeline>();

                    foreach (var entity in process.Entities) {

                        var pipeline = ctx.ResolveNamed<IPipeline>(entity.Key);

                        pipeline.Register(ctx.ResolveNamed<IEntityReader>(entity.Key));

                        pipeline.Register(new DefaultNulls(new PipelineContext(process, entity, null, entity.GetDefaultOf<Transform>(t => { t.Method = "defaultnulls"; }))));
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

        private static ITransform SwitchTransform(PipelineContext context) {
            switch (context.Transform.Method) {
                case "format": return new FormatTransform(context);
                case "left": return new LeftTransform(context);
                case "right": return new RightTransform(context);
                case "copy": return new CopyTransform(context);
                case "concat": return new ConcatTransform(context);
                case "fromxml": return new FromXmlTransform(context);
                case "fromsplit": return new FromSplitTransform(context);
                case "htmldecode": return new HtmlDecodeTransform(context);
                case "xmldecode": return new XmlDecodeTransform(context);
                case "hashcode": return new HashcodeTransform(context);
                case "padleft": return new PadLeftTransform(context);
                case "padright": return new PadRightTransform(context);
                case "splitlength": return new SplitLengthTransform(context);

                case "contains": return new ContainsValidater(context);
                case "is": return new IsValidator(context);
            }
            return new NullTransformer(context);
        }
    }
}