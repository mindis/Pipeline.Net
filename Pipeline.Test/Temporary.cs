using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;

namespace Pipeline.Test {

    [TestFixture(Description = "Proof of concept")]
    public class Temporary {

        [Test(Description = "Entity Pipeline")]
        public void EntityPipeline() {

            var pipelines = new TemporaryProcessPipelineComposer().Compose();
            var person = pipelines.First().Run().ToArray();

            Assert.AreEqual(3, person.Length);

            foreach (var row in person) {
                Console.WriteLine(row);
            }

            var pet = pipelines.Last().Run().ToArray();
            Assert.AreEqual(2, pet.Count());

            foreach (var row in pet) {
                Console.WriteLine(row);
            }
        }
    }

    public class TemporaryProcessPipelineComposer {

        public IPipeline[] Compose() {
            var builder = new ContainerBuilder();
            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var module = new PipelineModule(cfg, shorthand);
            if (module.Root.Errors().Any()) {
                foreach (var error in module.Root.Errors()) {
                    Console.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            builder.RegisterModule(module);
            var container = builder.Build();

            var pipelines = new List<IPipeline>();
            foreach (var process in module.Root.Processes) {
                pipelines.AddRange(container.ResolveNamed<IEnumerable<IPipeline>>(process.Key));
            }

            return pipelines.ToArray();
        }

    }

}
