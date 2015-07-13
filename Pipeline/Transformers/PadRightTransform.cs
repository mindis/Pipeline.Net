using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class PadRightTransform : BaseTransform, ITransform {
        private readonly Field _input;

        public PadRightTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().PadRight(Context.Transform.TotalWidth, Context.Transform.PaddingChar);
            Increment();
            return row;
        }

    }
}