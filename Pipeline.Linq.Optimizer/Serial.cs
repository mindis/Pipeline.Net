using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;

namespace Pipeline.Linq.Optimizer {

    public class Serial : DefaultPipeline {

        public new IEnumerable<Row> Run() {
            return Output.AsQueryExpr().Run();
        }
    }
}