using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;

namespace Pipeline.Linq.Optimizer {

    public class Parallel : DefaultPipeline {
        public Parallel(IEntityController controller)
            : base(controller) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsParallelQueryExpr().Run();
        }
    }
}