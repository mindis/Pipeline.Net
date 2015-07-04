using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public class FormatTransformer : BaseTransformer, ITransformer {
        private readonly Transform _transform;
        private readonly Field[] _input;

        public FormatTransformer(Process process, Entity entity, Field field, Transform transform)
            : base(process, entity, field) {
            _transform = transform;
            _input = GetInput(transform).ToArray();
        }

        public Row Transform(Row row) {
            row[Field] = string.Format(_transform.Format, _input.Select(f => row[f]).ToArray());
            return row;
        }

        Transform ITransformer.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {

            var split = SplitArguments(args);

            if (split.Length != 1) {
                problems.Add("format takes 1 parameter: a string template with place-holders in it.");
                return Guard();
            }

            if (!split[0].Contains("{")) {
                problems.Add("A format place-holder is required. There are no left curly brackets.");
                return Guard();
            }

            if (!split[0].Contains("}")) {
                problems.Add("A format place-holder is required. There are no right curly brackets.");
                return Guard();
            }

            return Configuration(t => {
                t.Method = "format";
                t.Format = split[0];
                t.IsShortHand = true;
            });
        }

    }
}