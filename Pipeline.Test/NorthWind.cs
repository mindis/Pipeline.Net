using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Interfaces;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture(Description = "Local Northwind Integration Testing")]
    public class Northwind {

        [Test]
        //[Ignore("Integration testing")]
        public void NorthwindIntegrationTesting() {

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(@"Files\Northwind.xml", File.ReadAllText(@"Files\Shorthand.xml")));
            var container = builder.Build();

            var root = container.Resolve<Root>();

            if (root.Errors().Any()) {
                foreach (var error in root.Errors()) {
                    Console.Error.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            if (root.Warnings().Any()) {
                foreach (var warning in root.Warnings()) {
                    Console.WriteLine(warning);
                }
            }

            var module = new PipelineModule(root, Logging.LogLevel.Debug);

            builder = new ContainerBuilder();
            builder.RegisterModule(module);
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

            //Assert.AreEqual(20088, output.Count());

            //foreach (var row in output.Take(10)) {
            //    Console.WriteLine(row);
            //}
        }
    }


}
