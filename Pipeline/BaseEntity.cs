using Pipeline.Configuration;

namespace Pipeline {
    public class BaseEntity {
        private long _rowCount;
        public Connection Connection { get; set; }
        public PipelineContext Context { get; private set; }

        public BaseEntity(PipelineContext context) {
            Context = context;
        }

        public void Increment(int by = 1) {
            _rowCount += by;
            if (_rowCount % Context.Entity.LogInterval == 0) {
                Context.Info(_rowCount.ToString());
            }
        }

    }
}