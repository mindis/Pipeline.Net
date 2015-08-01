using System;
using System.Collections.Generic;
using Nessos.LinqOptimizer.CSharp;
using Pipeline.Transformers;

namespace Pipeline.Linq.Optimizer {

    public class Serial : IEntityPipeline {
        IEntityPipeline _pipeline;
        public Serial(IEntityPipeline pipeline) {
            _pipeline = pipeline;
        }

        public void Execute() {
            _pipeline.Execute();
        }

        public void Initialize() {
            _pipeline.Initialize();
        }

        public void Register(IUpdate updater) {
            _pipeline.Register(updater);
        }

        public void Register(IWrite writer) {
            _pipeline.Register(writer);
        }

        public void Register(ITransform transformer) {
            _pipeline.Register(transformer);
        }

        public void Register(IRead reader) {
            _pipeline.Register(reader);
        }

        public IEnumerable<Row> Run() {
            return _pipeline.Run().AsQueryExpr().Run();
        }

    }
}