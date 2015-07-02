namespace Pipeline.Transformers {
    public class RightTransformer : ITransformer {
        private readonly int _length;
        private readonly IField[] _fields;

        public RightTransformer(int length, IField[] fields) {
            _length = length;
            _fields = fields;
        }

        public Row Transform(Row row) {
            for (int i = 0; i < _fields.Length; i++) {
                row[i] = row[i].ToString().Right(_length);
            }
            return row;
        }
    }
}