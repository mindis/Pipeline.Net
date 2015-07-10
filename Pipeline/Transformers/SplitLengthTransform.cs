using System.Collections.Generic;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Transformers {
    public class SplitLengthTransform : BaseTransform, ITransform {
        private readonly Field _input;
        private readonly char[] _separator;

        public SplitLengthTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _separator = context.Transform.Separator.ToCharArray();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input].ToString().Split(_separator).Length;
            Increment();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        static public Transform InterpretShorthand(string args, List<string> problems) {
            var split = SplitArguments(args);

            if (split.Length != 1) {
                problems.Add("The splitlength transform requires a single parameter: a separator.");
                return Guard();
            }

            return DefaultConfiguration(t => {
                t.Method = "splitlength";
                t.Separator = split[0];
            });
        }
    }
}