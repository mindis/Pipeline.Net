using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;
using Pipeline.Transformers;

namespace Pipeline.Validators {
    public class ContainsValidate : BaseTransform, ITransform {

        private readonly Field _input;
        private readonly Func<object, object> _contains;


        public ContainsValidate(Process process, Entity entity, Field field, Transform transform, IPipelineLogger logger)
            : base(process, entity, field, transform, logger) {

            _input = ParametersToFields().First();

            if (field.Type.StartsWith("bool")) {
                _contains = o => o.ToString().Contains(transform.Value);
            } else {
                _contains = o => o.ToString().Contains(transform.Value) ? String.Empty : String.Format("{0} does not contain {1}.", _input.Alias, transform.Value);
            }

            Name = string.Format("{0} contains {1}", _input.Alias, transform.Value);
        }

        public Row Transform(Row row) {
            row[Field] = _contains(row[_input]);
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