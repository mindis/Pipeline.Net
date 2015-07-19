using System.Collections.Generic;
using System.Linq;

namespace Pipeline {

    public class DataSetEntityReader : BaseEntityReader, IEntityReader {

        public DataSetEntityReader(PipelineContext context)
            : base(context) {
        }

        public IEnumerable<Row> Read(object min) {
            return GetTypedDataSet(Context.Entity.Name);
        }

        public object GetVersion() {
            return null;
        }

        public IEnumerable<Row> GetTypedDataSet(string name) {
            var rows = new List<Row>();
            var dataSet = Context.Process.DataSets.FirstOrDefault(ds => ds.Name == name);

            if (dataSet == null)
                return rows;

            var lookup = Context.Entity.Fields.ToDictionary(k => k.Name, v => v);
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