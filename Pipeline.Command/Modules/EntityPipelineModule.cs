using System;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Desktop;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Transformers;
using Pipeline.Transformers.System;

namespace Pipeline.Command.Modules {
    public partial class EntityPipelineModule : EntityModule {

        public EntityPipelineModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            var type = process.Pipeline == "defer" ? entity.Pipeline : process.Pipeline;

            builder.Register<IPipeline>((ctx) => {
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                IPipeline pipeline;
                switch (type) {
                    case "parallel.linq":
                        context.Debug("Registering {0} pipeline.", type);
                        pipeline = new ParallelPipeline(new DefaultPipeline(ctx.ResolveNamed<IEntityController>(entity.Key), context));
                        break;
                    default:
                        context.Debug("Registering linq pipeline.", type);
                        pipeline = new DefaultPipeline(ctx.ResolveNamed<IEntityController>(entity.Key), context);
                        break;
                }

                var provider = process.Connections.First(c => c.Name == "output").Provider;

                // extract
                pipeline.Register(ctx.ResolveNamed<IRead>(entity.Key));

                // transform
                pipeline.Register(new DefaultTransform(context, context.GetAllEntityFields()));
                pipeline.Register(new TflHashCodeTransform(context));
                pipeline.Register(TransformFactory.GetTransforms(ctx,process, entity, entity.GetAllFields().Where(f=>f.Transforms.Any())));
                pipeline.Register(new StringTruncateTransfom(context));

                if (provider == "sqlserver") {
                    pipeline.Register(new MinDateTransform(context, new DateTime(1753, 1, 1)));
                }

                //load
                pipeline.Register(ctx.ResolveNamed<IWrite>(entity.Key));
                pipeline.Register(ctx.ResolveNamed<IUpdate>(entity.Key));
                return pipeline;

            }).Named<IPipeline>(entity.Key);
        }
    }
}