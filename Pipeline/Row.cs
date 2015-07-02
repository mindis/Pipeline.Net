namespace Pipeline {
    public class Row : IRow {
        private readonly object[] _storage;

        public Row(int capacity) {
            _storage = new object[capacity - 1];
        }

        public object this[int index] {
            get {
                return _storage[index];
            }
            set {
                _storage[index] = value;
            }
        }

        public object this[IField field] {
            get { return _storage[field.Index]; }
            set { _storage[field.Index] = value; }
        }

        public override string ToString() {
            return string.Join("|", _storage);
        }
    }
}