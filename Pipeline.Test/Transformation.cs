using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;
using Pipeline.Linq;

namespace Pipeline.Test {

    [TestFixture]
    public class Transformation {


        [Test(Description = "Format Transformation")]
        public void FormatTransformer() {

            var root = new Root(File.ReadAllText(@"Files\PersonAndPet.xml"));

            var pipeline = new Serial();
            

        }
    }
}
