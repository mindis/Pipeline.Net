using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using System;
using System.Linq;
using Pipeline.Command;

namespace Pipeline.Test {
    public class PipelineComposer {

        public Root Root { get; set; }
        public Process Process { get; set; }

        public IProcessController Compose(string cfg, string mode = "default") {

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(cfg, @"Files\Shorthand.xml"));
            var container = builder.Build();

            Root = container.Resolve<Root>();

            if (Root.Errors().Any()) {
                foreach (var error in Root.Errors()) {
                    Console.Error.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            Process = Root.Processes.First();

            builder = new ContainerBuilder();
            builder.RegisterModule(new PipelineModule(Root, LogLevel.Info));
            container = builder.Build();

            return container.ResolveNamed<IProcessController>(Process.Key);
        }

    }

}
