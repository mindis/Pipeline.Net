using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;
using Pipeline.Logging;

namespace Pipeline.Linq.Optimizer {

    public class Parallel : DefaultPipeline {
        public Parallel(IPipelineLogger logger)
            : base(logger) {
        }

        public new IEnumerable<Row> Run() {
            return Output.AsParallelQueryExpr().Run();
        }
    }
}