using System.Collections.Generic;
using Nessos.Streams;
using Nessos.Streams.CSharp;
using Pipeline.Transformers;

namespace Pipeline.Streams {

    public class Parallel : BasePipeline, IPipeline {

        private ParStream<Row> _output;

        public void Input(IEntityReader entityReader) {
            _output = entityReader.Read().AsParStream();
        }

        public void Register(ITransformer transformer) {
            _output = _output.Select(transformer.Transform);
        }

        public IEnumerable<Row> Run() {
            return _output.Stream().ToEnumerable();
        }
    }
}