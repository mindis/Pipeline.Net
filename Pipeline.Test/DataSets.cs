using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class DataSets {

        [Test(Description = "A DataSet can be stored in an configuration, typed, and enumerated through.")]
        public void GetTypedDataSet() {

            var process = new Root(File.ReadAllText(@"Files\PersonAndPet.xml")).Processes.First();
            var name = process.DataSets.First().Name;
            var rows = process.GetTypedDataSet(name).ToArray();

            Assert.IsInstanceOf<IEnumerable<Row>>(rows);
            Assert.AreEqual(3, rows.Length);

            var dale = rows[0];
            var micheal = rows[1];
            Assert.IsInstanceOf<int>(dale[0]);
            Assert.AreEqual(1, dale[0]);
            Assert.AreEqual("Dale", dale[1]);
            Assert.AreEqual("Michael", micheal[1]);

            foreach (var row in rows) {
                Console.WriteLine(row);
            }
        }
    }
}
