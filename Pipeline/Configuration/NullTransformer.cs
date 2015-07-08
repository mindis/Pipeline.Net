using System;
using System.Collections.Generic;
using Pipeline.Logging;
using Pipeline.Transformers;

namespace Pipeline.Configuration {
    public class NullTransformer : BaseTransform, ITransform {
        public NullTransformer(Process process, Entity entity, Field field, Transform transform, IPipelineLogger logger)
            : base(process, entity, field, transform, logger) {
        }

        public Row Transform(Row row) {
            return row;
        }

        public Transform InterpretShorthand(string args, List<string> problems) {
            return Guard();
        }
    }
}