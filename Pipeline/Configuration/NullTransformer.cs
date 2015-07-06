using System;
using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline.Configuration {
    public class NullTransformer : BaseTransform, ITransform {
        public NullTransformer(Process process, Entity entity, Field field)
            : base(process, entity, field) {
        }

        public Row Transform(Row row) {
            return row;
        }

        public Transform InterpretShorthand(string args, List<string> problems) {
            throw new NotImplementedException();
        }
    }
}