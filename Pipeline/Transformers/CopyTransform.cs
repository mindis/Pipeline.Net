using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Transformers {

    public class CopyTransform : BaseTransform, ITransform {

        private readonly Field _input;
        public CopyTransform(Process process, Entity entity, Field field, Transform transform, IPipelineLogger logger)
            : base(process, entity, field, transform, logger) {
            _input = ParametersToFields().First();
            Name = "copy";
        }

        public Row Transform(Row row) {
            row[Field] = row[_input];
            Increment();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            if (string.IsNullOrEmpty(args)) {
                problems.Add("The copy method requires at least one parameter referencing a field.");
                return Guard();
            }

            var element = DefaultConfiguration(t => {
                t.Method = "copy";
                t.IsShortHand = true;
            });

            var split = Shared.Split(args, ",");

            if (split.Length == 1) {
                element.Parameter = split[0];
                return element;
            }

            foreach (var p in split) {
                var parameter = element.GetDefaultOf<Parameter>();
                if (p.Contains(":")) {
                    //named values
                    var named = p.Split(':');
                    parameter.Name = named[0];
                    parameter.Value = named[1];
                } else if (p.Contains(".")) {
                    // entity, field combinations
                    var dotted = p.Split('.');
                    parameter.Entity = dotted[0];
                    parameter.Field = dotted[1];
                } else {
                    parameter.Field = p; // just fields
                }
                element.Parameters.Add(parameter);
            }
            return element;
        }
    }
}