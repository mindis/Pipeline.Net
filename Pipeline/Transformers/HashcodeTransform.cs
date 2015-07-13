using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public class HashcodeTransform : BaseTransform, ITransform {
        private readonly Field[] _input;
        public HashcodeTransform(PipelineContext context)
            : base(context) {
            _input = MultipleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = string.Concat(_input.Select(f => row[f])).GetHashCode();
            Increment();
            return row;
        }

    }
}