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
            var pet = pipelines.Run().ToArray();
            Assert.AreEqual(2, pet.Count());

            foreach (var row in pet) {
                Console.WriteLine(row);
            }
        }

    }

}
