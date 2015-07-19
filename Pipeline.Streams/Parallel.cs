using System.Collections.Generic;
using Nessos.Streams.CSharp;
using Pipeline.Logging;

namespace Pipeline.Streams {

    public class Parallel : DefaultPipeline {

        public Parallel(IEntityController controller)
            : base(controller) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsParStream().Stream().ToEnumerable();
        }
    }
}