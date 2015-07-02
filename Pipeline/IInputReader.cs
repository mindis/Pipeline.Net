using System.Collections.Generic;

namespace Pipeline {
    public interface IInputReader {
        IEnumerable<Row> Read();
    }
}