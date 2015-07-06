using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;

namespace Pipeline {
    public class DefaultPipeline : IPipeline {

        public virtual IEnumerable<Row> Output { get; protected set; }

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