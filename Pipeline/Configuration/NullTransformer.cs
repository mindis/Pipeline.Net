using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline.Configuration {
    public class NullTransformer : BaseTransform, ITransform {
        public NullTransformer(PipelineContext context)
            : base(context) {
        }

        public Row Transform(Row row) {
            return row;
        }

    }
}