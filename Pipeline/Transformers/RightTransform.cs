using System.Collections.Generic;
using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline.Transformers {
    public class RightTransform : BaseTransform, ITransform {

        public RightTransform(Process process, Entity entity, Field field, Transform transform)
            : base(process, entity, field, transform) {
        }

        public Row Transform(Row row) {
            row[Field] = row[Field].ToString().Right(Configuration.Length);
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            int length;

            if (!int.TryParse(args, out length)) {
                problems.Add(string.Format("The right method requires a single integer representing the length, or how many right-most characters you want. You passed in '{0}'.", args));
                return Guard();
            }

            return DefaultConfiguration(t => {
                t.Method = "right";
                t.Length = length;
                t.IsShortHand = true;
            });
        }
    }
}