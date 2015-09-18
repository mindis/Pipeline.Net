using System;
using System.Linq;
using NUnit.Framework;

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
