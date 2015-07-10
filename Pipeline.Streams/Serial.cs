using System.Collections.Generic;
using System.Linq;
using Nessos.Streams;
using Nessos.Streams.CSharp;
using Pipeline.Logging;

namespace Pipeline.Streams {

    public class Serial : DefaultPipeline {

        public Serial(IPipelineLogger logger)
            : base(logger) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsStream().ToEnumerable();
        }
    }
}