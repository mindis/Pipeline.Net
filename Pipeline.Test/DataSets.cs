using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Test {

    [TestFixture]
    public class DataSets {

        static Field FieldAt(short index) {
            return new Field { Index = index, MasterIndex = index };
        }

        [Test(Description = "A DataSet can be stored in an configuration, typed, and enumerated through.")]
        public void GetTypedDataSet()
        {

            var cfg = File.ReadAllText(@"Files\PersonAndPet.xml");
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var process = new Root(cfg, shorthand).Processes.First();
            var personContext = new PipelineContext(new DebugLogger(), process, process.Entities.Last());
            var rows = new DataSetEntityReader(personContext).Read().ToArray();
            
            Assert.IsInstanceOf<IEnumerable<Row>>(rows);
            Assert.AreEqual(3, rows.Length);

            var dale = rows[0];
            var micheal = rows[1];
            Assert.IsInstanceOf<int>(dale[FieldAt(0)]);
            Assert.AreEqual(1, dale[FieldAt(0)]);
            Assert.AreEqual("Dale", dale[FieldAt(1)]);
            Assert.AreEqual("Michael", micheal[FieldAt(1)]);

            foreach (var row in rows) {
                Console.WriteLine(row);
            }
        }
    }
}
