using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class SplitLengthTransform : BaseTransform, ITransform {
        readonly Field _input;
        readonly char[] _separator;

        public SplitLengthTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _separator = context.Transform.Separator.ToCharArray();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row.GetString(_input).Split(_separator).Length;
            Increment();
            return row;
        }

    }
}