using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class ConcatTransform : BaseTransform, ITransform {
        private readonly Field[] _input;

        public ConcatTransform(PipelineContext context)
            : base(context) {
            _input = MultipleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = string.Concat(_input.Select(f => row[f]));
            Increment();
            return row;
        }

    }
}