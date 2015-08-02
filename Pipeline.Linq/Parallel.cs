using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;

namespace Pipeline.Linq {

    public class Parallel : IEntityPipeline {
        IEntityPipeline _pipeline;

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
            return _pipeline.Run().AsParallel();
        }
    }
}