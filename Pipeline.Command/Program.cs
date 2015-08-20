using Autofac;
using Pipeline.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Pipeline.Command {
    class Program {
        static void Main(string[] args) {

            var context = new PipelineContext(new ConsoleLogger(Logging.LogLevel.Debug), new Configuration.Process() { Name = "Command" });

            if(args == null || args.Length == 0) {
                context.Error("Please pass in a configuration.");
                Environment.Exit(1);
            }

            // create module and check configuration, could check to make sure files exist
            var cfg = File.ReadAllText(new FileInfo(args[0]).FullName);
            var shortHand = File.ReadAllText(new FileInfo("Shorthand.xml").FullName);

            var module = new PipelineModule(cfg, shortHand);
            if (module.Root.Warnings().Any()) {
                foreach (var warning in module.Root.Warnings()) {
                    context.Warn(warning);
                }
            }
            if (module.Root.Errors().Any()) {
                foreach (var error in module.Root.Errors()) {
                    context.Error(error);
                }
                Environment.Exit(1);
            }
            context.Info("Configuration is Ok");

            // register
            var builder = new ContainerBuilder();
            builder.RegisterModule(module);
            var container = builder.Build();

            // resolve and run
            foreach (var process in module.Root.Processes) {
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
