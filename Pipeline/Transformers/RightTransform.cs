using System.Collections.Generic;
using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline.Transformers {
    public class RightTransform : BaseTransform, ITransform {
        private readonly int _length;

        public RightTransform(Process process, Entity entity, Field field, Configuration.Transform transform)
            : base(process, entity, field) {
            _length = transform.Length;
        }

        public Row Transform(Row row) {
            row[Field] = row[Field].ToString().Right(_length);
            return row;
        }

        Configuration.Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Configuration.Transform InterpretShorthand(string args, List<string> problems) {
            int length;

            if (!int.TryParse(args, out length)) {
                problems.Add(string.Format("The right method requires a single integer representing the length, or how many right-most characters you want. You passed in '{0}'.", args));
                return Guard();
            }

            return Configuration(t => {
                t.Method = "right";
                t.Length = length;
                t.IsShortHand = true;
            });
        }
    }
}