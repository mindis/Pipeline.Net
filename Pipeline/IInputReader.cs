using System.Collections.Generic;
using Pipeline.Logging;

namespace Pipeline {
    public interface IEntityReader {
        IPipelineLogger Logger { get; set; }
        IEnumerable<Row> Read();
        PipelineContext Context { get; }
    }
}