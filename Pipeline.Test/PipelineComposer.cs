using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using System;
using System.Linq;
using Pipeline.Command;
using Pipeline.Command.Modules;

namespace Pipeline.Test {
    public class PipelineComposer {

        public Root Root { get; set; }
        public Process Process { get; set; }

        public IProcessController Compose(string cfg, LogLevel logLevel = LogLevel.Debug) {

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

            if (Root.Warnings().Any()) {
                foreach (var warning in Root.Warnings()) {
                    Console.Error.WriteLine(warning);
                }
            }

            Process = Root.Processes.First();

            builder = new ContainerBuilder();
            builder.Register<IPipelineLogger>(ctx => new ConsoleLogger(logLevel)).SingleInstance();
            builder.RegisterModule(new MapModule(Root));
            builder.RegisterModule(new TemplateModule(Root));
            builder.RegisterModule(new ActionModule(Root));
            builder.RegisterModule(new EntityControlModule(Root));
            builder.RegisterModule(new EntityPipelineModule(Root));
            builder.RegisterModule(new EntityInputModule(Root));
            builder.RegisterModule(new EntityOutputModule(Root));
            builder.RegisterModule(new EntityMasterUpdateModule(Root));
            builder.RegisterModule(new EntityDeleteModule(Root));
            builder.RegisterModule(new EntityPipelineModule(Root));
            builder.RegisterModule(new ProcessPipelineModule(Root));
            builder.RegisterModule(new ProcessControlModule(Root));

            container = builder.Build();

            return container.ResolveNamed<IProcessController>(Process.Key);
        }

    }

}
