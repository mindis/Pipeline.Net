using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Desktop;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;

namespace Pipeline.Command.Modules {
    public class EntityDeleteModule : EntityModule {
        public EntityDeleteModule(Root root) : base(root) {
        }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            if (entity.Delete) {
                builder.Register<IEntityDeleteHandler>(ctx => {
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);

                    var inputConnection = process.Connections.First(c => c.Name == entity.Connection);

                    IRead input = new NullReader(context);
                    switch (inputConnection.Provider) {
                        case "sqlserver":
                            input = new SqlReader(context, entity.GetPrimaryKey(), ReadFrom.Input);
                            break;
                    }

                    IRead output = new NullReader(context);
                    IDelete deleter = new NullDeleter(context);
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
}