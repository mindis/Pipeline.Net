using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;
using Pipeline.Logging;

namespace Pipeline.Linq.Optimizer {

    public class Serial : DefaultPipeline {
        public Serial(IPipelineLogger logger)
            : base(logger) {
        }

        public override IEnumerable<Row> Run() {
            return base.Run().AsQueryExpr().Run();
        }
    }
}