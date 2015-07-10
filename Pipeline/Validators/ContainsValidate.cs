using System;
using System.Collections.Generic;
using Pipeline.Configuration;
using Pipeline.Logging;
using Pipeline.Transformers;

namespace Pipeline.Validators {
    public class ContainsValidater : BaseTransform, ITransform {

        private readonly Field _input;
        private readonly Func<object, object> _contains;

        public ContainsValidater(PipelineContext context)
            : base(context) {

            _input = SingleInput();

            if (context.Field.Type.StartsWith("bool")) {
                _contains = o => o.ToString().Contains(context.Transform.Value);
            } else {
                _contains = o => o.ToString().Contains(context.Transform.Value) ? String.Empty : String.Format("{0} does not contain {1}.", _input.Alias, context.Transform.Value);
            }

        }

        public Row Transform(Row row) {
            row[Context.Field] = _contains(row[_input]);
            Increment();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            var split = SplitArguments(args);
            if (split.Length != 1) {
                problems.Add("The contains validator requires a value to search for.");
                return Guard();
            }

            return DefaultConfiguration(a => {
                a.Method = "contains";
                a.Value = args;
            });
        }
    }
}