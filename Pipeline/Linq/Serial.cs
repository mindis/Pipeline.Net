using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;

namespace Pipeline.Linq {

    public class Serial : BasePipeline, IPipeline {

        private IEnumerable<Row> _output;

        public void Input(IInputReader reader) {
            _output = reader.Read();
        }

        public void Register(ITransformer transformer) {
            _output = _output.Select(transformer.Transform);
        }

        public IEnumerable<Row> Run() {
            return _output;
        }
    }
}