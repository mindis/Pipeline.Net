using Pipeline.Extensions;

namespace Pipeline.Transformers {

    public class LeftTransform : BaseTransform, ITransform {

        private readonly int _length;
        private readonly IField _input;

        public LeftTransform(PipelineContext context)
            : base(context) {
            _length = context.Transform.Length;
            _input = SingleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().Left(_length);
            Increment();
            return row;
        }

    }
}