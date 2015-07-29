using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class TrimStartTransform : BaseTransform, ITransform {
        readonly Field _input;
        readonly char[] _trimChars;

        public TrimStartTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _trimChars = Context.Transform.TrimChars.ToCharArray();
        }

        public Row Transform(Row row) {
            row.SetString(Context.Field, row.GetString(_input).TrimStart(_trimChars));
            Increment();
            return row;
        }
    }
}