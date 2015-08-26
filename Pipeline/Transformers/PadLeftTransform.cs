using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class PadLeftTransform : BaseTransform, ITransform {
        readonly Field _input;

        public PadLeftTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
        }

        public Row Transform(Row row) {
            row.SetString(Context.Field, row.GetString(_input).PadLeft(Context.Transform.TotalWidth, Context.Transform.PaddingChar));
            Increment();
            return row;
        }

    }
}