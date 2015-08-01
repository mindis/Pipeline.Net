using System.Collections.Generic;
using System.Linq;

namespace Pipeline {

    public class DataSetEntityReader : IRead {
        EntityInput _input;

        public DataSetEntityReader(EntityInput input) {
            _input = input;
        }

        public IEnumerable<Row> Read() {
            return GetTypedDataSet(_input.Context.Entity.Name);
        }

        public object GetVersion() {
            return null;
        }

        public IEnumerable<Row> GetTypedDataSet(string name) {
            var rows = new List<Row>();
            var dataSet = _input.Context.Process.DataSets.FirstOrDefault(ds => ds.Name == name);

            if (dataSet == null)
                return rows;

            var lookup = _input.Context.Entity.Fields.ToDictionary(k => k.Name, v => v);
            foreach (var row in dataSet.Rows) {
                var pipelineRow = new Row(_input.RowCapacity, _input.Context.Entity.IsMaster);
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