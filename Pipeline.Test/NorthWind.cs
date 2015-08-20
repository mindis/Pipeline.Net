using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Interfaces;

namespace Pipeline.Test {

    [TestFixture(Description = "Local Northwind Integration Testing")]
    public class Northwind {

        [Test]
        //[Ignore("Integration testing")]
        public void NorthwindIntegrationTesting() {

            var northwind = File.ReadAllText(@"Files\Northwind.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var module = new PipelineModule(northwind, shorthand, Logging.LogLevel.Debug);

            if (module.Root.Errors().Any()) {
                foreach (var error in module.Root.Errors()) {
                    Console.Error.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            if (module.Root.Warnings().Any()) {
                foreach (var warning in module.Root.Warnings()) {
                    Console.WriteLine(warning);
                }
            }

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

            //Assert.AreEqual(20088, output.Count());

            //foreach (var row in output.Take(10)) {
            //    Console.WriteLine(row);
            //}
        }
    }


}
