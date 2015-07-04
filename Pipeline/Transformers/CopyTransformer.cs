using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public class CopyTransformer : BaseTransformer, ITransformer {

        private readonly Field _input;
        public CopyTransformer(Process process, Entity entity, Field field, Transform transform)
            : base(process, entity, field) {
            _input = GetInput(transform).First();
        }

        public Row Transform(Row row) {
            row[Field] = row[_input];
            return row;
        }

        Transform ITransformer.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            if (string.IsNullOrEmpty(args)) {
                problems.Add("The copy method requires at least one parameter referencing a field.");
                return Guard();
            }

            var element = Configuration(t => {
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