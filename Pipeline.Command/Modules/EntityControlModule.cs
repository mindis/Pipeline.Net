using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;

namespace Pipeline.Command.Modules {

    public class EntityControlModule : EntityModule {

        public EntityControlModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            builder.Register<IEntityController>(ctx => {
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                var output = new OutputContext(context, new Incrementer(context));

                switch (output.Connection.Provider) {
                    case "sqlserver":
                        context.Debug("Registering sql server controller");
                        var initializer = process.Mode == "init" ? (IAction)new SqlEntityInitializer(output) : new NullInitializer();
                        return new SqlEntityController(output, initializer);
                    default:
                        context.Debug("Registering null controller");
                        return new NullEntityController();
                }

            }).Named<IEntityController>(entity.Key);
        }
    }
}