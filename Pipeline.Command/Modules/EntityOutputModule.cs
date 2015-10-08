using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;

namespace Pipeline.Command.Modules {
    public class EntityOutputModule : EntityModule {
        public EntityOutputModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {

            builder.Register<IWrite>((ctx) => {
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                var incrementer = new Incrementer(context);
                var output = new OutputContext(context, incrementer);
                switch (output.Connection.Provider) {
                    case "sqlserver":
                        context.Debug("Registering Sql Server writer");
                        return new SqlEntityBulkInserter(output);
                    default:
                        context.Warn("Registering null writer", output.Connection.Provider);
                        return new NullWriter(context);
                }
            }).Named<IWrite>(entity.Key);
        }
    }
}