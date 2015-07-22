using System.Collections.Generic;

namespace Pipeline {

    public interface IEntityWriter {
        void Write(IEnumerable<Row> rows);
        void LoadVersion();
        PipelineContext Context { get; }
    }
}