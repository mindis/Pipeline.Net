using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class PadLeftTransform : BaseTransform, ITransform {
        private readonly Field _input;

        public PadLeftTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().PadLeft(Context.Transform.TotalWidth, Context.Transform.PaddingChar);
            Increment();
            return row;
        }

    }
}