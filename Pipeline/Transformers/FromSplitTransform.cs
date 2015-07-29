using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class FromSplitTransform : BaseTransform, ITransform {

        readonly char[] _separator;
        readonly Field _input;
        readonly Field[] _output;

        public FromSplitTransform(PipelineContext context)
            : base(context) {
            _input = SingleInputForMultipleOutput();
            _output = MultipleOutput();
            _separator = context.Transform.Separator.ToCharArray();
        }

        public Row Transform(Row row) {
            var values = row.GetString(_input).Split(_separator);
            if (values.Length > 0) {
                for (var i = 0; i < values.Length && i < _output.Length; i++) {
                    var output = _output[i];
                    row[output] = output.Convert(values[i]);
                }
            }
            Increment();
            return row;
        }

    }
}