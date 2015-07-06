using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;

namespace Pipeline.Linq.Optimizer {

    public class Parallel : DefaultPipeline {

        public new IEnumerable<Row> Run() {
            return Output.AsParallelQueryExpr().Run();
        }
    }
}