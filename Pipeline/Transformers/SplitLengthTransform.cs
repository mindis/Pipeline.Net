using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class SplitLengthTransform : BaseTransform, ITransform {
        private readonly Field _input;
        private readonly char[] _separator;

        public SplitLengthTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _separator = context.Transform.Separator.ToCharArray();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().Split(_separator).Length;
            Increment();
            return row;
        }

    }
}