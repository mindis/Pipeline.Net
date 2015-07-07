using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class HashcodeTransform : BaseTransform, ITransform {
        private readonly Field[] _input;
        public HashcodeTransform(Process process, Entity entity, Field field, Transform transform)
            : base(process, entity, field, transform) {
            _input = ParametersToFields().ToArray();
        }

        public Row Transform(Row row) {
            row[Field] = string.Concat(_input.Select(f => row[f])).GetHashCode();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            return Parameterless("hashcode", "hashed", args, problems);
        }
    }
}