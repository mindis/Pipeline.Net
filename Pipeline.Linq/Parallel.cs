using System.Collections.Generic;
using System.Linq;
using Pipeline.Logging;

namespace Pipeline.Linq {

    public class Parallel : DefaultPipeline {
        public Parallel(IPipelineLogger logger)
            : base(logger) {
        }

        public new IEnumerable<Row> Run() {
            return Output.AsParallel();
        }
    }
}