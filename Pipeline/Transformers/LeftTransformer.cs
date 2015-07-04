
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline.Transformers {
    public class LeftTransformer : BaseTransformer, ITransformer {

        private readonly int _length;
        private readonly IField _input;

        public LeftTransformer(Process process, Entity entity, Field field, Transform transform) : base(process, entity, field) {
            _length = transform.Length;
            _input = GetInput(transform).First();
        }

        public Row Transform(Row row) {
            row[Field] = row[_input].ToString().Left(_length);
            return row;
        }

        Transform ITransformer.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            int length;
            if (!int.TryParse(args, out length)) {
                problems.Add(string.Format("The left method requires a single integer representing the length, or how many left-most characters you want. You passed in '{0}'.", args));
                return Guard();
            }

            return Configuration(t => {
                t.Method = "left";
                t.Length = length;
                t.IsShortHand = true;
            });
        }
    }
}