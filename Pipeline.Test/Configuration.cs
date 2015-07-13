using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class Configuration {

        [Test(Description = "Cfg-Net can read Files\\PersonAndPet.xml")]
        public void ConfurationIsGood() {
            var module = new PipelineModule(File.ReadAllText(@"Files\PersonAndPet.xml"));

            foreach (var error in module.Root.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, module.Root.Errors().Count());
            Assert.AreEqual(0, module.Root.Warnings().Count());
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

            Assert.AreEqual(0, pet.Fields[0].Index);
            Assert.AreEqual(1, pet.Fields[1].Index);
            Assert.AreEqual(2, pet.Fields[2].Index);
            Assert.AreEqual(3, pet.Fields[3].Index);
            Assert.AreEqual(4, pet.Fields[4].Index);
            Assert.AreEqual(5, pet.CalculatedFields[0].Index);
            Assert.AreEqual(6, pet.CalculatedFields[1].Index);
        }
    }
}
