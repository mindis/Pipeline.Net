using System.Collections.Generic;
using Nessos.Streams.CSharp;
using Pipeline.Logging;

namespace Pipeline.Streams {

    public class Parallel : DefaultPipeline {

        public Parallel(IPipelineLogger logger)
            : base(logger) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsParStream().Stream().ToEnumerable();
        }
    }
}