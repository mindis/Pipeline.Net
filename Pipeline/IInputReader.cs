using System.Collections.Generic;

namespace Pipeline {
    public interface IEntityReader {
        IEnumerable<Row> Read();
    }
}