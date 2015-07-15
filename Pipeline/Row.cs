using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class Row : IRow {
        private readonly object[] _storage;

        public Row(int capacity) {
            _storage = new object[capacity];
        }

        public object this[IField field] {
            get { return _storage[field.Index]; }
            set { _storage[field.Index] = value; }
        }

        public override string ToString() {
            return string.Join("|", _storage);
        }

        public ExpandoObject ToExpandoObject(Field[] fields, int batchId) {
            var parameters = new ExpandoObject();
            var dict = ((IDictionary<string, object>)parameters);
            foreach (var field in fields) {
                dict.Add("f" + field.Index, _storage[field.Index]);
            }
            dict.Add("TflBatchId", batchId);
            return parameters;
        }

        public object[] ToObjectArray(Field[] fields, int batchId) {
            var output = new List<object>(fields.Length + 1);
            output.AddRange(fields.Select(field => _storage[field.Index]));
            output.Add(batchId);
            return output.ToArray();
        }
    }
}