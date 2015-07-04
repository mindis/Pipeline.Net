using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;
using Pipeline.Linq;

namespace Pipeline.Test {

    [TestFixture(Description = "Proof of concept")]
    public class Temporary {

        [Test(Description = "Entity Pipeline")]
        public void EntityPipeline() {

            var process = new Root(File.ReadAllText(@"Files\PersonAndPet.xml")).Processes.First();
            var pipelines = new TemporaryProcessPipelineComposer(process).Compose();
            var output = pipelines.First().Run();

            Assert.AreEqual(3, output.Count());
        }
    }

    public class TemporaryProcessPipelineComposer {
        private readonly Process _process;

        public TemporaryProcessPipelineComposer(Process process) {
            _process = process;
        }

        public IPipeline[] Compose() {
            var pipelines = new List<IPipeline>();
            foreach (var entity in _process.Entities) {
                var pipeline = new Serial();
                pipeline.Input(new DataSetEntityReader(_process, entity));
                foreach (var field in entity.GetAllFields()) {
                    foreach (var transform in field.Transforms) {
                        pipeline.Register(transform.GetTransformer(_process, entity, field));
                    }
                }
                pipelines.Add(pipeline);
            }
            return pipelines.ToArray();
        }
    }
}
