using System.Collections.Generic;

namespace Pipeline {
    public interface IEntityWriter {
        void Write(IEnumerable<Row> rows, int batchId);
        object GetVersion();
        PipelineContext Context { get; }
    }
}