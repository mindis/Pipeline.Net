using System.Collections.Generic;
using System.Linq;
using Pipeline.Interfaces;

namespace Pipeline {
    public class NullReader : IRead {
        public IEnumerable<Row> Read() {
            return Enumerable.Empty<Row>();
        }
    }
}