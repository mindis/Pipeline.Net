using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;
using Pipeline.Linq;
using Pipeline.Transformers;

namespace Pipeline.Test {

    [TestFixture]
    public class Transform {

        [Test(Description = "Format Transformation")]
        public void FormatTransformer() {

            var process = new Root(File.ReadAllText(@"Files\PersonAndPet.xml")).Processes.First();
            var pipeline = new DefaultPipeline();
            var input = new DataSetEntityReader(process, process.Entities.First()).Read();
            var calculatedField = process.Entities.First().CalculatedFields.First();
            pipeline.Input(input);
            pipeline.Register(new FormatTransform(process, process.Entities.First(), calculatedField, calculatedField.Transforms.First()));
            var output = pipeline.Run().ToArray();

            Assert.AreEqual("Dale Edward Jones", output[0][calculatedField]);

        }
    }
}
