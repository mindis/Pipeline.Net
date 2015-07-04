using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class DataSets {

        private static Field FieldAt(int index) {
            return new Field { Index = index };
        }

        [Test(Description = "A DataSet can be stored in an configuration, typed, and enumerated through.")]
        public void GetTypedDataSet() {

            var process = new Root(File.ReadAllText(@"Files\PersonAndPet.xml")).Processes.First();
            var rows = new DataSetEntityReader(process, process.Entities.First()).Read().ToArray();

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
