using System.Collections.Generic;

namespace Pipeline {
    public interface IEntityReader {
        IEnumerable<Row> Read(object min);
        PipelineContext Context { get; }
    }
}