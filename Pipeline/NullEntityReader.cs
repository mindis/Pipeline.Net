using Pipeline.Interfaces;
using System.Collections.Generic;

namespace Pipeline {
    public class NullEntityReader : IRead {
        public IEnumerable<Row> Read() {
            return new Row[0];
        }

    }
}