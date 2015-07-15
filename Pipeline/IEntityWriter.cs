using System.Collections.Generic;
using Pipeline.Logging;

namespace Pipeline {
    public interface IEntityWriter {
        void Write(IEnumerable<Row> rows);
        PipelineContext Context { get; }
    }
}