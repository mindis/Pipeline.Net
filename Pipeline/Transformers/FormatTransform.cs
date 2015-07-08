using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Transformers {

    public class FormatTransform : BaseTransform, ITransform {

        private readonly Field[] _input;

        public FormatTransform(Process process, Entity entity, Field field, Transform transform, IPipelineLogger logger)
            : base(process, entity, field, transform, logger) {
            _input = ParametersToFields().ToArray();
            Name = "format";
            }

        public Row Transform(Row row) {
            row[Field] = string.Format(Configuration.Format, _input.Select(f => row[f]).ToArray());
            Increment();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
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

            return DefaultConfiguration(t => {
                t.Method = "format";
                t.Format = split[0];
                t.IsShortHand = true;
            });
        }

    }
}