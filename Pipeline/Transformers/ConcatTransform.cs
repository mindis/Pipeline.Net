using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class ConcatTransform : BaseTransform, ITransform {
        private readonly Field[] _input;

        public ConcatTransform(Process process, Entity entity, Field field, Configuration.Transform transform)
            : base(process, entity, field) {
            _input = ParametersToFields(transform).ToArray();
        }

        public Row Transform(Row row) {
            row[Field] = string.Concat(_input.Select(f => row[f]));
            return row;
        }

        Configuration.Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Configuration.Transform InterpretShorthand(string args, List<string> problems) {
            return Parameterless("concat", "concatenated", args, problems);
        }
    }
}