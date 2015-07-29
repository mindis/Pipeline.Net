using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class PadRightTransform : BaseTransform, ITransform {
        readonly Field _input;

        public PadRightTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
        }

        public Row Transform(Row row) {
            row.SetString(Context.Field, row.GetString(_input).PadRight(Context.Transform.TotalWidth, Context.Transform.PaddingChar));
            Increment();
            return row;
        }

    }
}