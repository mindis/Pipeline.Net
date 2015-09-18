using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;
using Pipeline.Transformers;
using Pipeline.Interfaces;

namespace Pipeline.Linq.Optimizer {

    public class Parallel : IEntityPipeline {
        readonly IEntityPipeline _pipeline;

        public Parallel(IEntityPipeline pipeline) {
            _pipeline = pipeline;
        }
        public void Execute() {
            _pipeline.Execute();
        }

        public void Initialize() {
            _pipeline.Initialize();
        }

        public void Register(IEnumerable<ITransform> transforms) {
            _pipeline.Register(transforms);
        }

        public void Register(IUpdate updater) {
            _pipeline.Register(updater);
        }

        public void Register(IWriteOutput writer) {
            _pipeline.Register(writer);
        }

        public void Register(ITransform transformer) {
            _pipeline.Register(transformer);
        }

        public void Register(IReadInput reader) {
            _pipeline.Register(reader);
        }

        public IEnumerable<Row> Run() {
            return _pipeline.Run().AsParallelQueryExpr().Run();
        }
    }
}