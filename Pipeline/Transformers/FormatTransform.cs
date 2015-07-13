using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Transformers {

    public class FormatTransform : BaseTransform, ITransform {

        private readonly Field[] _input;

        public FormatTransform(PipelineContext context)
            : base(context) {
            _input = MultipleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = string.Format(Context.Transform.Format, _input.Select(f => row[f]).ToArray());
            Increment();
            return row;
        }

    }
}