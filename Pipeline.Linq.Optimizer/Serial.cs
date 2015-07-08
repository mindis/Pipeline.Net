using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;
using Pipeline.Logging;

namespace Pipeline.Linq.Optimizer {

    public class Serial : DefaultPipeline {
        public Serial(IPipelineLogger logger)
            : base(logger) {
        }

        public new IEnumerable<Row> Run() {
            return Output.AsQueryExpr().Run();
        }
    }
}