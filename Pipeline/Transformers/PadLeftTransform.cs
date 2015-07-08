using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Transformers {
    public class PadLeftTransform : BaseTransform, ITransform {
        private readonly Field _input;

        public PadLeftTransform(Process process, Entity entity, Field field, Transform transform, IPipelineLogger logger)
            : base(process, entity, field, transform, logger) {
            _input = ParametersToFields().First();
            Name = "padleft";
            }

        public Row Transform(Row row) {
            row[Field] = row[_input].ToString().PadLeft(Configuration.TotalWidth, Configuration.PaddingChar);
            Increment();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            return Pad("padleft", args, problems);
        }
    }
}