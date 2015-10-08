using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;

namespace Pipeline.Command.Modules {
    public class EntityMasterUpdateModule : EntityModule {
        public EntityMasterUpdateModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            //master updater
            builder.Register<IUpdate>((ctx) => {
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                var incrementer = new Incrementer(context);
                var output = new OutputContext(context, incrementer);
                switch (output.Connection.Provider) {
                    case "sqlserver":
                        context.Debug("Registering {0} master updater", output.Connection.Provider);
                        return new SqlMasterUpdater(output);
                    default:
                        context.Warn("Registering null updater");
                        return new NullMasterUpdater();
                }
            }).Named<IUpdate>(entity.Key);
        }
    }
}