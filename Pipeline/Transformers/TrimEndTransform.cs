using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class TrimEndTransform : BaseTransform, ITransform {
        private readonly Field _input;
        private readonly char[] _trimChars;

        public TrimEndTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _trimChars = Context.Transform.TrimChars.ToCharArray();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().TrimEnd(_trimChars);
            Increment();
            return row;
        }
    }
}