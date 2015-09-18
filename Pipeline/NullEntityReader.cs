using Pipeline.Interfaces;
using System.Collections.Generic;

namespace Pipeline {
    public class NullEntityReader : IReadInput {
        public void LoadVersion() {
            // not supported for this reader
        }

        public IEnumerable<Row> Read() {
            return new Row[0];
        }

    }
}