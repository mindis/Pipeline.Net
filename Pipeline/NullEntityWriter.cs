using System.Collections.Generic;

namespace Pipeline {
    public class NullEntityWriter : IEntityWriter {

        public PipelineContext Context { get; private set; }

        public void Initialize() {}

        public NullEntityWriter(PipelineContext context) {
            Context = context;
        }

        public void Write(IEnumerable<Row> rows, int batchId) {}

        public object GetVersion() {
            return null;
        }

    }
}