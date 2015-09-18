using System.Collections.Generic;
using Pipeline.Interfaces;

namespace Pipeline.Test {
    public class TestReader : IRead {
        public List<Row> Data { get; }

        public TestReader(List<Row> rows) {
            Data = rows;
        }

        public IEnumerable<Row> Read() {
            return Data;
        }
    }
}