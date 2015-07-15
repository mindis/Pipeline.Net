using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class TrimTransform : BaseTransform, ITransform {
        private readonly Field _input;
        private readonly char[] _trimChars;

        public TrimTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _trimChars = Context.Transform.TrimChars.ToCharArray();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().Trim(_trimChars);
            Increment();
            return row;
        }
    }
}