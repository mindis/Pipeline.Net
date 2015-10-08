using Pipeline.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline {

    public class DataSetEntityReader : IRead {
        readonly InputContext _input;

        public DataSetEntityReader(InputContext input) {
            _input = input;
        }

        public IEnumerable<Row> Read() {
            return GetTypedDataSet(_input.Entity.Name);
        }

        public object GetVersion() {
            return null;
        }

        public IEnumerable<Row> GetTypedDataSet(string name) {
            var rows = new List<Row>();
            var dataSet = _input.Process.DataSets.FirstOrDefault(ds => ds.Name == name);

            if (dataSet == null)
                return rows;

            var lookup = _input.Entity.Fields.ToDictionary(k => k.Name, v => v);
            foreach (var row in dataSet.Rows) {
                var pipelineRow = new Row(_input.RowCapacity, _input.Entity.IsMaster);
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