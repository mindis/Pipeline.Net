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
        //readonly bool _isMaster;
        readonly Func<IField, short> _index;

        //public bool Exists { get; set; }
        //public bool Delta { get; set; }
        public int TflHashCode { get; set; }

        //public Row(Row row, bool exists, bool delta) {
        //    _storage = row._storage;
        //    _stringStorage = row._stringStorage;
        //    Exists = exists;
        //    Delta = delta;
        //    if (_isMaster) {
        //        _index = (f) => { return f.MasterIndex; };
        //    } else {
        //        _index = (f) => { return f.Index; };
        //    }
        //}

        public Row(int capacity, bool isMaster) {
            //_isMaster = isMaster;
            _storage = new object[capacity];
            _stringStorage = new string[capacity];
            if (isMaster) {
                _index = (f) => { return f.MasterIndex; };
            } else {
                _index = (f) => { return f.Index; };
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
            if (fields.Length > 1)
                return fields.Select(f => this[f]).SequenceEqual(fields.Select(f => other[f]));
            return this[fields[0]].Equals(other[fields[0]]);
        }

    }
}