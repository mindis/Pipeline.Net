using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class TrimStartTransform : BaseTransform, ITransform {
        private readonly Field _input;
        private readonly char[] _trimChars;

        public TrimStartTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _trimChars = Context.Transform.TrimChars.ToCharArray();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().TrimStart(_trimChars);
            Increment();
            return row;
        }
    }
}