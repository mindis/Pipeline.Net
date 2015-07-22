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

        public ExpandoObject ToExpandoObject(Field[] fields) {
            var parameters = new ExpandoObject();
            var dict = ((IDictionary<string, object>)parameters);
            foreach (var field in fields) {
                dict.Add("f" + field.Index, _storage[field.Index]);
            }
            return parameters;
        }

        public IEnumerable<object> ToEnumerable(Field[] fields) {
            return fields.Select(f => _storage[f.Index]);
        }

    }
}