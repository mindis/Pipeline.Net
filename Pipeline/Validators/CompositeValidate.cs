using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;

namespace Pipeline.Validators {
    public class CompositeValidator : BaseTransform, ITransform {
        readonly IEnumerable<ITransform> _transforms;
        readonly Func<Row, object> _validate;

        public CompositeValidator(PipelineContext context, IEnumerable<ITransform> transforms)
            : base(context) {
            _transforms = transforms.ToArray();

            if (context.Field.Type.StartsWith("bool", StringComparison.Ordinal)) {
                _validate = r => _transforms.All(t => (bool)t.Transform(r)[context.Field]);
            } else {
                _validate = r => string.Concat(_transforms.Select(t => t.Transform(r)[context.Field] + " ")).Trim();
            }
        }

        public Row Transform(Row row) {
            row[Context.Field] = _validate(row);
            Increment();
            return row;
        }

    }
}