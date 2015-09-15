using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Logging;
using Pipeline.Interfaces;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class Inventory {

        [Test]
        [Ignore("Integration testing")]
        public void InventoryTesting() {

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(@"C:\temp\Inventory.xml?Mode=init", @"C:\Code\Pipeline.Net\Pipeline.Test\Files\Shorthand.xml"));
            var container = builder.Build();

            var root = container.Resolve<Root>();

            if (root.Errors().Any()) {
                foreach (var error in root.Errors()) {
                    Console.Error.WriteLine(error);
                }
            }

            builder = new ContainerBuilder();
            builder.RegisterModule(new PipelineModule(root, LogLevel.Debug));

            using (var scope = builder.Build().BeginLifetimeScope()) {
                foreach (var process in root.Processes) {
                    var controller = scope.ResolveNamed<IProcessController>(process.Key);
                    controller.PreExecute();
                    controller.Execute();
                    controller.PostExecute();
                }
            }

        }
    }


}
