using Pipeline.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline {
    public class ProcessController : IProcessController {

        readonly IInitializer _initializer;
        private readonly IEnumerable<IEntityPipeline> _entityPipelines;

        public ProcessController(
            IInitializer initializer,
            IEnumerable<IEntityPipeline> entityPipelines
        ) {
            _initializer = initializer;
            _entityPipelines = entityPipelines;
        }

        public void PreExecute() {
            _initializer.Initialize();
        }

        public void Execute() {
            foreach (var entity in _entityPipelines) {
                entity.Initialize();
                entity.Execute();
            }
        }

        public void PostExecute() {
            // todo, e.g. create star view for relational outputs
        }

        public IEnumerable<Row> Run() {
            // todo, flatten and send all entity data back, for now, take first entity
            return _entityPipelines.First().Run();
        }
    }
}
