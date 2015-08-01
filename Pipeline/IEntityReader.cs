using System.Collections.Generic;

namespace Pipeline {
    public interface IRead {
        IEnumerable<Row> Read();
    }
}