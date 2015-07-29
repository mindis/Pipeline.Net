using System;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Validators {
    public class IsValidator : BaseTransform, ITransform {
      readonly Field _input;
      readonly Func<string, object> _canConvert;

      public IsValidator(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            if (context.Field.Type.StartsWith("bool", StringComparison.Ordinal)) {
                _canConvert = v => Constants.CanConvert()[context.Transform.Type](v);
            } else {
                _canConvert = v => Constants.CanConvert()[context.Transform.Type](v) ? string.Empty : string.Format("The value {0} can not be converted to a {1}.", v, context.Transform.Type);
            }
        }

        public Row Transform(Row row) {
            row[Context.Field] = _canConvert(row.GetString(_input));
            Increment();
            return row;
        }

    }
}