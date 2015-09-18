using System.Collections.Generic;

namespace Pipeline.Interfaces {
    public interface IRead {
        IEnumerable<Row> Read();
    }
}