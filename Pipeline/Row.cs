using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Pipeline.Configuration;
using System;
using Pipeline.Interfaces;

namespace Pipeline {
    public class Row : IRow {
        readonly object[] _storage;
        readonly string[] _stringStorage;
        readonly Func<IField, short> _index;

        public int TflHashCode { get; set; }

        public Row(int capacity, bool isMaster) {
            _storage = new object[capacity];
            _stringStorage = new string[capacity];
            if (isMaster) {
                _index = (f) => f.MasterIndex;
            } else {
                _index = (f) => f.Index;
            }
        }

        public object this[IField field] {
            get { return _storage[_index(field)]; }
            set { _storage[_index(field)] = value; }
        }

        public string GetString(IField field) {
            var i = _index(field);
            return _stringStorage[i] ?? _storage[i].ToString();
        }

        public void SetString(IField field, string value) {
            var i = _index(field);
            _stringStorage[i] = value;
            _storage[i] = value;
        }

        public override string ToString() {
            return string.Join("|", _storage);
        }

        public ExpandoObject ToExpandoObject(Field[] fields) {
            var parameters = new ExpandoObject();
            var dict = ((IDictionary<string, object>)parameters);
            foreach (var field in fields) {
                dict.Add(field.FieldName(), _storage[_index(field)]);
            }
            return parameters;
        }

        public IEnumerable<object> ToEnumerable(Field[] fields) {
            return fields.Select(f => _storage[_index(f)]);
        }

        public bool Match(Field[] fields, Row other) {
            return fields.Length > 1 ?
                fields.Select(f => this[f]).SequenceEqual(fields.Select(f => other[f])) :
                this[fields[0]].Equals(other[fields[0]]);
        }
    }
}