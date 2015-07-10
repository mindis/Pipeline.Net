using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline {
    public class BaseEntityReader {

        private long _rowCount;

        public BaseEntityReader(PipelineContext context) {
            Context = context;
            RowCapacity = context.Entity.GetAllFields().Count();
            InputFields = context.Entity.Fields.Where(f => f.Input).ToArray();
            Logger = new NullLogger();
        }

        public IPipelineLogger Logger { get; set; }
        public int RowCapacity { get; set; }
        public Field[] InputFields { get; set; }
        public PipelineContext Context { get; private set; }

        public void Increment() {
            _rowCount++;
            if (_rowCount % Context.Entity.LogInterval == 0) {
                Logger.Info(Context, _rowCount.ToString());
            }
        }
    }
}
