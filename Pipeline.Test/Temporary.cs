using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Interfaces;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Test {

    [TestFixture(Description = "Proof of concept")]
    public class Temporary {

        [Test(Description = "Entity Pipeline")]
        public void EntityPipeline() {

            var pipelines = new PipelineComposer().Compose(@"Files\PersonAndPet.xml");
            var person = pipelines.EntityPipelines.Last().Run().ToArray();

            Assert.AreEqual(3, person.Length);

            foreach (var row in person) {
                Console.WriteLine(row);
            }

            var pet = pipelines.EntityPipelines.First().Run().ToArray();
            Assert.AreEqual(2, pet.Count());

            foreach (var row in pet) {
                Console.WriteLine(row);
            }
        }

    }

}
