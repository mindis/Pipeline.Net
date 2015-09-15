using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class Configuration {

        [Test(Description = @"Cfg-Net can read Files\PersonAndPet.xml")]
        public void ConfurationIsGood() {
            var composer = new PipelineComposer();
            var controller = composer.Compose(@"Files\PersonAndPet.xml");

            Assert.AreEqual(0, composer.Root.Errors().Count());
            Assert.AreEqual(0, composer.Root.Warnings().Count());
        }


        [Test(Description = "Process populates index for each field")]
        public void FieldsAreIndexed() {

            var composer = new PipelineComposer();
            composer.Compose(@"Files\PersonAndPet.xml");

            var pet = composer.Process.Entities.First();
            var person = composer.Process.Entities.Last();

            Assert.AreEqual(0, pet.Fields[0].Index);
            Assert.AreEqual(1, pet.Fields[1].Index);
            Assert.AreEqual(2, pet.Fields[2].Index);
            Assert.AreEqual(3, pet.Fields[3].Index);
            Assert.AreEqual(4, pet.Fields[4].Index);
            Assert.AreEqual(5, pet.CalculatedFields[0].Index);
            Assert.AreEqual(6, pet.CalculatedFields[1].Index);

            Assert.AreEqual("Id", person.Fields[0].Name);
            Assert.AreEqual(0, person.Fields[0].Index);
            Assert.AreEqual(1, person.Fields[1].Index);
            Assert.AreEqual(2, person.Fields[2].Index);
            Assert.AreEqual(3, person.Fields[3].Index);
            Assert.AreEqual(4, person.CalculatedFields[0].Index);
        }

        [Test(Description = "Process populates key types")]
        public void KeysTypesSet() {

            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");

            var root = new Root(cfg, shorthand, new Cfg.Net.Validators("js", new JintParser()));

            foreach (var error in root.Errors()) {
                Console.WriteLine(error);
            }

            foreach (var warning in root.Warnings()) {
                Console.WriteLine(warning);
            }

            var pet = root.Processes.First().Entities.First();
            Assert.AreEqual(KeyType.Primary, pet.Fields[0].KeyType);
            Assert.AreEqual(KeyType.None, pet.Fields[1].KeyType);
            Assert.IsTrue(pet.Fields[4].KeyType.HasFlag(KeyType.Foreign));

            var person = root.Processes.First().Entities.Last();
            Assert.IsTrue(person.Fields[0].KeyType == KeyType.Primary, "Person Id is a primary key on Person table.");
            Assert.IsTrue(person.Fields.Skip(1).All(f => f.KeyType == KeyType.None), "All other Person fields are KeyType.None.");

        }

        [Test]
        public void TestRelationshipToMaster() {
            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var root = new Root(cfg, shorthand, new Cfg.Net.Validators("js", new JintParser()));
            var rtm = root.Processes[0].Entities[1].RelationshipToMaster;

            Assert.AreEqual(1, rtm.Count());

        }

    }
}
