using System.Collections.Generic;
using Nessos.Streams.CSharp;
using Pipeline.Logging;

namespace Pipeline.Streams {

    public class Serial : DefaultPipeline {

        public Serial(IEntityController controller)
            : base(controller) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsStream().ToEnumerable();
        }
    }
}