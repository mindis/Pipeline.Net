using System.Collections.Generic;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Desktop.Transformers;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Transformers;
using Pipeline.Validators;

namespace Pipeline.Command.Modules {
    public static class TransformFactory {

        public static IEnumerable<ITransform> GetTransforms(IComponentContext ctx, Process process, Entity entity, IEnumerable<Field> fields) {
            var transforms = new List<ITransform>();
            foreach (var f in fields.Where(f => f.Transforms.Any())) {
                var field = f;
                if (field.RequiresCompositeValidator()) {
                    transforms.Add(new CompositeValidator(
                        new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field),
                        field.Transforms.Select(t => SwitchTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t) { Activity = PipelineActivity.Transform }))
                        ));
                } else {
                    transforms.AddRange(field.Transforms.Select(t => SwitchTransform(ctx, new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t) { Activity = PipelineActivity.Transform })));
                }
            }
            return transforms;
        }

        public static ITransform SwitchTransform(IComponentContext ctx, PipelineContext context) {
            context.Activity = PipelineActivity.Transform;
            switch (context.Transform.Method) {
                case "format": return new FormatTransform(context);
                case "left": return new LeftTransform(context);
                case "right": return new RightTransform(context);
                case "copy": return new CopyTransform(context);
                case "concat": return new ConcatTransform(context);
                case "fromxml": return new FromXmlTransform(context);
                case "fromsplit": return new FromSplitTransform(context);
                case "htmldecode": return new DecodeTransform(context);
                case "xmldecode": return new DecodeTransform(context);
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
                case "map": return new MapTransform(context, ctx.ResolveNamed<IMapReader>(context.Process.Maps.First(m => m.Name == context.Transform.Map).Key));
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