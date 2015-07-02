using System.Collections.Generic;
using Nessos.Streams;
using Nessos.Streams.CSharp;
using Pipeline.Transformers;

namespace Pipeline.Streams {

    public class Serial : BasePipeline, IPipeline {

        private Stream<Row> _output;

        public void Input(IInputReader reader) {
            _output = reader.Read().AsStream();
        }

        public void Register(ITransformer transformer) {
            _output = _output.Select(transformer.Transform);
        }

        public IEnumerable<Row> Run() {
            return _output.ToEnumerable();
        }
    }
}