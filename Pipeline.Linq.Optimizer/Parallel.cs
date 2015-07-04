using System.Collections.Generic;
using System.Linq;
using Nessos.LinqOptimizer.CSharp;
using Pipeline.Transformers;

namespace Pipeline.Linq.Optimizer {

    public class Parallel : BasePipeline, IPipeline {

        private IEnumerable<Row> _output;

        public void Input(IEntityReader entityReader) {
            _output = entityReader.Read();
        }

        public void Register(ITransformer transformer) {
            _output = _output.Select(transformer.Transform);
        }

        public IEnumerable<Row> Run() {
            return _output.AsParallelQueryExpr().Run();
        }
    }
}