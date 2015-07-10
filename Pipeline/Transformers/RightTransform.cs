using System.Collections.Generic;
using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline.Transformers {
    public class RightTransform : BaseTransform, ITransform {

        public RightTransform(PipelineContext context)
            : base(context) {
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[Context.Field].ToString().Right(Context.Transform.Length);
            Increment();
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