using System.Linq;

namespace Pipeline.Transformers {

    public class FormatTransformer : ITransformer {
        private readonly IField _output;
        private readonly string _format;
        private readonly IField[] _input;

        public FormatTransformer(IField output, string format, IField[] input) {
            _output = output;
            _format = format;
            _input = input;
        }

        public Row Transform(Row row) {
            row[_output] = string.Format(_format, _input.Select(f => row[(IField)f]));
            return row;
        }
    }
}