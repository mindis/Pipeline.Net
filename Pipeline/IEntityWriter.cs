using System.Collections.Generic;

namespace Pipeline {
    public interface IEntityWriter {
        void Write(IEnumerable<Row> rows);
        PipelineContext Context { get; }
        void Initialize();
    }
}