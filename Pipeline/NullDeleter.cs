using System.Collections.Generic;
using Pipeline.Interfaces;

namespace Pipeline {
    public class NullDeleter : IDelete {
        public void Delete(IEnumerable<Row> rows) {
            // not really deleting
        }
    }
}