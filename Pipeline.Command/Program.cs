using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Pipeline.Command {
    class Program {
        static void Main(string[] args) {

            var context = new PipelineContext(new ConsoleLogger(Logging.LogLevel.Debug), new Configuration.Process() { Name = "Command" });

            if (args == null || args.Length == 0) {
                context.Error("Please pass in a configuration.");
                System.Environment.Exit(1);
            }

            var shortHand = File.ReadAllText(new FileInfo("Shorthand.xml").FullName);

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(args[0], shortHand));
            var container = builder.Build();

            var root = container.Resolve<Root>();

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

            // register pipeline
            builder = new ContainerBuilder();
            builder.RegisterModule(new PipelineModule(root));
            container = builder.Build();

            // resolve and run
            foreach (var process in root.Processes) {
                var controller = container.ResolveNamed<IProcessController>(process.Key);
                controller.Initialize();
                foreach (var pipeline in controller.EntityPipelines) {
                    pipeline.Initialize();
                    pipeline.Execute();
                }
            }

            // release
            container.Dispose();
        }
    }
}
