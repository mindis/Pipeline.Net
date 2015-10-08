using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;

namespace Pipeline.Command.Modules {
    public class EntityInputModule : EntityModule {

        public EntityInputModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            builder.Register<IRead>(ctx => {
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                var input = new InputContext(context, new Incrementer(context));
                switch (input.Connection.Provider) {
                    case "internal":
                        context.Debug("Registering {0} provider", input.Connection.Provider);
                        return new DataSetEntityReader(input);
                    case "sqlserver":
                        if (input.Entity.ReadSize == 0) {
                            context.Debug("Registering {0} reader", input.Connection.Provider);
                            return new SqlInputReader(input, input.InputFields);
                        }
                        context.Debug("Registering {0} batch reader", input.Connection.Provider);
                        return new SqlInputBatchReader(
                            input,
                            new SqlInputReader(input, input.Entity.GetPrimaryKey())
                            );
                    default:
                        context.Warn("Registering null reader", input.Connection.Provider);
                        return new NullEntityReader();
                }
            }).Named<IRead>(entity.Key);

        }
    }
}