using System.Collections.Generic;

namespace Pipeline {
    public class NullEntityReader : IEntityReader {

        public PipelineContext Context { get; private set; }

        public NullEntityReader(PipelineContext context) {
            Context = context;
        }

        public IEnumerable<Row> Read(object min) {
            return new Row[0];
        }

        public object GetVersion() {
            return null;
        }

    }
}