using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            var split = SplitArguments(args);
            if (split.Length != 1) {
                problems.Add("The is(type) validator requires a type argument.");
                return Guard();
            }

            if (!Constants.TypeSet().Contains(split[0])) {
                problems.Add(string.Format("The type {0} is not recognized.  Valid types are {1}.", split[0], Constants.TypeDomain));
                return Guard();
            }

            return DefaultConfiguration(a => {
                a.Method = "is";
                a.Type = split[0];
                a.To = split[0];
            });
        }
    }
}