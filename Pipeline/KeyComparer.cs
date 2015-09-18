using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class KeyComparer : IEqualityComparer<Row> {
        private readonly Field[] _keys;
        private readonly Func<Row, int> _hasher;

        public KeyComparer(Field[] keys) {
            _keys = keys;
            if (_keys.Length == 1) {
                _hasher = row => row[_keys[0]].GetHashCode();
            } else {
                _hasher = row => {
                    unchecked {
                        return _keys.Aggregate(17, (current, key) => current * 23 + row[key].GetHashCode());
                    }
                };
            }
        }

        public bool Equals(Row x, Row y) {
            return x.Match(_keys, y);
        }

        public int GetHashCode(Row obj) {
            return _hasher(obj);
        }

    }
}