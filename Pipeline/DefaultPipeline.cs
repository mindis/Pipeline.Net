using System.Collections.Generic;
using System.Linq;
using Pipeline.Logging;
using Pipeline.Transformers;

namespace Pipeline {
    public class DefaultPipeline : IPipeline {

        public IPipelineLogger Logger { get; private set; }
        public virtual IEnumerable<Row> Output { get; protected set; }

        public DefaultPipeline(IPipelineLogger logger) {
            Logger = logger;
        }

        public virtual void Input(IEnumerable<Row> input) {
            Output = input;
        }

        public virtual void Register(ITransform transformer) {
            Output = Output.Select(transformer.Transform);
        }

        public virtual IEnumerable<Row> Run() {
            return Output;
        }
    }
}