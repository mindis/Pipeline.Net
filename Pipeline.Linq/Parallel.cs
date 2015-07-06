using System.Collections.Generic;
using System.Linq;

namespace Pipeline.Linq {

    public class Parallel : DefaultPipeline {

        public new IEnumerable<Row> Run() {
            return Output.AsParallel();
        }
    }
}