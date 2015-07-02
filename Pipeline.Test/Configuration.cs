using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class Configuration {

        [Test(Description = "Cfg-Net can read Files\\PersonAndPet.xml")]
        public void ConfurationIsGood() {
            var root = new Root(File.ReadAllText(@"Files\PersonAndPet.xml"));

            Assert.AreEqual(0, root.Errors().Count());
            Assert.AreEqual(0, root.Warnings().Count());
        }


        [Test(Description = "Process populates index for each field")]
        public void FieldsAreIndexed() {

            var root = new Root(File.ReadAllText(@"Files\PersonAndPet.xml"));

            var person = root.Processes.First().Entities.First();
            var pet = root.Processes.First().Entities.Last();

            Assert.AreEqual("Id", person.Fields[0].Name);
            Assert.AreEqual(0, person.Fields[0].Index);
            Assert.AreEqual(1, person.Fields[1].Index);
            Assert.AreEqual(2, person.Fields[2].Index);
            Assert.AreEqual(3, person.Fields[3].Index);
            Assert.AreEqual(4, person.CalculatedFields[0].Index);

            Assert.AreEqual(5, pet.Fields[0].Index);
            Assert.AreEqual(6, pet.Fields[1].Index);
            Assert.AreEqual(7, pet.Fields[2].Index);
            Assert.AreEqual(8, pet.Fields[3].Index);
            Assert.AreEqual(9, pet.Fields[4].Index);
            Assert.AreEqual(10, pet.Fields[5].Index);
            Assert.AreEqual(11, pet.Fields[6].Index);
        }
    }
}
