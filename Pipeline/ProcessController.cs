using Pipeline.Interfaces;
using System.Collections.Generic;

namespace Pipeline {
    public class ProcessController : IProcessController {
        public IEnumerable<IEntityPipeline> EntityPipelines { get; set; }

        readonly IInitializer _initializer;

        public ProcessController(IInitializer processInitializer, IEnumerable<IEntityPipeline> entityPipelines) {
            _initializer = processInitializer;
            EntityPipelines = entityPipelines;
        }

        public void Initialize() {
            _initializer.Initialize();
        }
    }
}
