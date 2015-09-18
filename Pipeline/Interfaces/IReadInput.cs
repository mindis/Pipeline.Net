using System.Collections.Generic;

namespace Pipeline.Interfaces {
    public interface IReadInput {
        IEnumerable<Row> Read();
        void LoadVersion();
    }
}