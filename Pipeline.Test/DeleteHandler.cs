using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using NUnit.Framework;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Test {

    [TestFixture]
    public class DeleteHandler {

        [Test]
        public void NoDeletes() {
            var entity = GetTestEntity();
            entity.MinVersion = 1;  // to make entity.IsFirstRun() == false for testing purposes
            var input = GetTestReader(entity);
            var output = GetTestReader(entity);
            var deleter = new TestDeleter(entity, output.Data);
            var deleteHandler = new DefaultDeleteHandler(entity, input, output, deleter);

            deleteHandler.Delete();
            Assert.AreEqual(2, deleter.Data.Count);
        }

        [Test]
        public void OneDelete() {
            var entity = GetTestEntity();
            entity.MinVersion = 1;  // to make entity.IsFirstRun() == false for testing purposes
            var input = GetTestReader(entity);
            var output = GetTestReader(entity);

            // remove one from input
            var last = input.Data.Last();
            input.Data.Remove(last);

            var deleter = new TestDeleter(entity, output.Data);
            var deleteHandler = new DefaultDeleteHandler(entity, input, output, deleter);

            deleteHandler.Delete();
            Assert.AreEqual(1, deleter.Data.Count, "output should loose one too");
        }

        [Test]
        public void DeleteAll() {
            var entity = GetTestEntity();
            entity.MinVersion = 1;  // to make entity.IsFirstRun() == false for testing purposes
            var input = GetTestReader(entity);
            var output = GetTestReader(entity);

            // remove one from input
            input.Data.Clear();

            var deleter = new TestDeleter(entity, output.Data);
            var deleteHandler = new DefaultDeleteHandler(entity, input, output, deleter);

            deleteHandler.Delete();
            Assert.AreEqual(0, deleter.Data.Count, "output should everthing too");
        }

        private static TestReader GetTestReader(Entity entity) {
            var row1 = new Row(2, false) {
                [entity.Fields[0]] = 1,
                [entity.Fields[1]] = "One"
            };
            var row2 = new Row(2, false) {
                [entity.Fields[0]] = 2,
                [entity.Fields[1]] = "Two"
            };

            var data = new List<Row> { row1, row2 };
            return new TestReader(data);
        }

        private static Entity GetTestEntity() {
            var process = new Process();
            return process.GetValidatedOf<Entity>(e => {
                e.Name = "e1";
                e.Delete = true;
                e.Fields = new List<Field> {
                process.GetValidatedOf<Field>(f => {
                    f.Index = 0;
                    f.Name = "f1";
                    f.Type = "int";
                    f.PrimaryKey = true;
                }),
                process.GetValidatedOf<Field>(f => {
                    f.Index = 1;
                    f.Name = "f2";
                    f.Type = "string";
                    f.Length = "64";
                })
            };
            });
        }
    }

    public class TestDeleter : IDelete {
        private readonly Entity _entity;
        public List<Row> Data { get; private set; }

        public TestDeleter(Entity entity, List<Row> data) {
            _entity = entity;
            Data = data;
        }

        public void Delete(IEnumerable<Row> rows) {
            Data = Data.Except(rows, new KeyComparer(_entity.GetPrimaryKey())).ToList();
        }
    }
}
