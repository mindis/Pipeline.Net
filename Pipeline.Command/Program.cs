using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using System.Linq;
using Pipeline.Command.Modules;
using Pipeline.Logging;

namespace Pipeline.Command {
    class Program {
        static void Main(string[] args) {

            var context = new PipelineContext(new ConsoleLogger(Logging.LogLevel.Debug), new Configuration.Process() { Name = "Command" });

            if (args == null || args.Length == 0) {
                context.Error("Please pass in a configuration.");
                System.Environment.Exit(1);
            }

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(args[0], "Shorthand.xml"));
            var cfg = builder.Build();

            var root = cfg.Resolve<Root>();

            if (root.Warnings().Any()) {
                foreach (var warning in root.Warnings()) {
                    context.Warn(warning);
                }
            }
            if (root.Errors().Any()) {
                foreach (var error in root.Errors()) {
                    context.Error(error);
                }
                System.Environment.Exit(1);
            }
            context.Info("Configuration is Ok");
            cfg.Dispose();

            // register pipeline
            builder = new ContainerBuilder();
            builder.Register<IPipelineLogger>(ctx => new ConsoleLogger(LogLevel.Info)).SingleInstance();
            builder.RegisterModule(new MapModule(root));
            builder.RegisterModule(new TemplateModule(root));
            builder.RegisterModule(new ActionModule(root));
            builder.RegisterModule(new EntityControlModule(root));
            builder.RegisterModule(new EntityInputModule(root));
            builder.RegisterModule(new EntityOutputModule(root));
            builder.RegisterModule(new EntityMasterUpdateModule(root));
            builder.RegisterModule(new EntityDeleteModule(root));
            builder.RegisterModule(new EntityPipelineModule(root));
            builder.RegisterModule(new ProcessPipelineModule(root));
            builder.RegisterModule(new ProcessControlModule(root));

            using (var c = builder.Build().BeginLifetimeScope()) {
                // resolve, run, and release
                var container = c;
                foreach (var controller in root.Processes.Select(process => container.ResolveNamed<IProcessController>(process.Key))) {
                    controller.PreExecute();
                    controller.Execute();
                    controller.PostExecute();
                }
            }
        }
    }
}
