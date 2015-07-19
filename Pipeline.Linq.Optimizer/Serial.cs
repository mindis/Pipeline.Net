using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;

namespace Pipeline.Linq.Optimizer {

    public class Serial : DefaultPipeline {
        public Serial(IEntityController controller)
            : base(controller) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsQueryExpr().Run();
        }
    }
}