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
            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var module = new PipelineModule(cfg, shorthand);

            foreach (var error in module.Root.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, module.Root.Errors().Count());
            Assert.AreEqual(0, module.Root.Warnings().Count());
        }


        [Test(Description = "Process populates index for each field")]
        public void FieldsAreIndexed() {

            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var root = new Root(cfg, shorthand);

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

        [Test(Description = "Process populates key types")]
        public void KeysTypesSet() {

            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var root = new Root(cfg, shorthand);

            foreach (var error in root.Errors()) {
                Console.WriteLine(error);
            }

            foreach (var warning in root.Warnings()) {
                Console.WriteLine(warning);
           } 

            var person = root.Processes.First().Entities.First();
            var pet = root.Processes.First().Entities.Last();

            Assert.AreEqual(KeyType.Primary, person.Fields[0].KeyType);
            Assert.IsTrue(person.Fields.Skip(1).All(f => f.KeyType == KeyType.None));

            Assert.AreEqual(KeyType.Primary, pet.Fields[0].KeyType);
            Assert.AreEqual(KeyType.None, pet.Fields[1].KeyType);
            Assert.IsTrue(pet.Fields[4].KeyType.HasFlag(KeyType.Foreign));
        }

        [Test]
        public void TestRelationshipToMaster() {
            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var root = new Root(cfg, shorthand);
            var rtm = root.Processes[0].Entities[1].RelationshipToMaster;

            Assert.AreEqual(1, rtm.Count());

        }

    }
}
