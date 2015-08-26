using Pipeline.Configuration;
using Jint;

namespace Pipeline.Transformers {
    public class JintTransform : BaseTransform, ITransform {
        readonly Field[] _input;
        readonly Engine _jint = new Engine();

        public JintTransform(PipelineContext context) : base(context) {
            _input = MultipleInput();
        }

        public Row Transform(Row row) {
            foreach (var field in _input) {
                _jint.SetValue(field.Alias, row[field]);
            }
            row[Context.Field] = _jint.Execute(Context.Transform.Script).GetCompletionValue().ToObject();
            Increment();
            return row;
        }
    }
}
