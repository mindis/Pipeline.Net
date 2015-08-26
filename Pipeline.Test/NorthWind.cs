using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Interfaces;
using Pipeline.Configuration;
using Pipeline.Provider.SqlServer;

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

            var pipe = new PipelineContext(new TraceLogger(), root.Processes.First());
            var context = new OutputContext(pipe, new Incrementer(pipe));
            var sql = context.SqlCreateStarView();


            Assert.IsNotNull(sql);

            //foreach (var process in root.Processes) {
            //    process.Mode = "init";
            //    foreach (var entity in process.Entities) {
            //        entity.Mode = "init";
            //    }
            //}

            var module = new PipelineModule(root, Logging.LogLevel.Info);

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
