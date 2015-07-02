using System.Collections.Generic;

namespace Pipeline {
    public class MemoryInputReader : IInputReader {
        private readonly Row[] _input;

        public MemoryInputReader(Row[] input) {
            _input = input;
        }

        public IEnumerable<Row> Read() {
            return _input;
        }
    }
}