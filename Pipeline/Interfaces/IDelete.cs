using System.Collections.Generic;

namespace Pipeline.Interfaces {
    public interface IDelete {
        void Delete(IEnumerable<Row> rows);
    }
}