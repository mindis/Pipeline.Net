using System;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Validators {
    public class IsValidator : BaseTransform, ITransform {
        private readonly Field _input;
        private readonly Func<string, object> _canConvert;

        public IsValidator(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            if (context.Field.Type.StartsWith("bool")) {
                _canConvert = v => Constants.CanConvert()[context.Transform.Type](v);
            } else {
                _canConvert = v => Constants.CanConvert()[context.Transform.Type](v) ? string.Empty : string.Format("The value {0} can not be converted to a {1}.", v, context.Transform.Type);
            }
        }

        public Row Transform(Row row) {
            row[Context.Field] = _canConvert(row[_input].ToString());
            Increment();
            return row;
        }

    }
}