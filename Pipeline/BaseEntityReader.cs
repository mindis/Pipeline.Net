using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline {
    public class BaseEntityReader {

        private long _rowCount;
        private readonly string _connectionName;

        public BaseEntityReader(Process process, Entity entity, IPipelineLogger logger) {
            Process = process;
            Entity = entity;
            Logger = logger;

            RowCapacity = entity.GetAllFields().Count();
            InputFields = entity.Fields.Where(f => f.Input).ToArray();
            Context = new PipelineContext(process, entity, null, null);
            _connectionName = process.Connections.First(c => c.Name == entity.Connection).Name;
        }

        public IPipelineLogger Logger { get; private set; }
        public int RowCapacity { get; set; }
        public Process Process { get; private set; }
        public Entity Entity { get; private set; }
        public Field[] InputFields { get; set; }
        public PipelineContext Context { get; private set; }

        public void Increment() {
            _rowCount++;
            if (_rowCount % Entity.LogInterval == 0) {
                Logger.Info(Context, "Read {0} rows from {1}", _rowCount, _connectionName);
            }
        }
    }
}
