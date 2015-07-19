using System.Collections.Generic;
using System.Linq;

namespace Pipeline.Linq {

    public class Parallel : DefaultPipeline {
        public Parallel(IEntityController controller)
            : base(controller) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsParallel();
        }
    }
}