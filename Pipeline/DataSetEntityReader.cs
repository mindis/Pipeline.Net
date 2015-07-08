using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline {

    public class DataSetEntityReader : BaseEntityReader, IEntityReader {

        public DataSetEntityReader(Process process, Entity entity, IPipelineLogger logger)
            : base(process, entity, logger) {
        }

        public IEnumerable<Row> Read() {
            return GetTypedDataSet(Entity.Name);
        }

        public IEnumerable<Row> GetTypedDataSet(string name) {
            var rows = new List<Row>();
            var dataSet = Process.DataSets.FirstOrDefault(ds => ds.Name == name);

            if (dataSet == null)
                return rows;

            var lookup = Entity.Fields.ToDictionary(k => k.Name, v => v);
            foreach (var row in dataSet.Rows) {
                var pipelineRow = new Row(RowCapacity);
                foreach (var pair in row) {
                    if (!lookup.ContainsKey(pair.Key))
                        continue;
                    var field = lookup[pair.Key];
                    pipelineRow[field] = field.Convert(pair.Value);
                }
                rows.Add(pipelineRow);
            }
            return rows;
        }
    }
}