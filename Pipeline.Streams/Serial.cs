using System.Collections.Generic;
using Nessos.Streams;
using Nessos.Streams.CSharp;
using Pipeline.Transformers;

namespace Pipeline.Streams {

    public class Serial : DefaultPipeline {

        private Stream<Row> _stream;

        public override void Input(IEnumerable<Row> input) {
            _stream = input.AsStream();
        }

        public override void Register(ITransform transformer) {
            _stream = _stream.Select(transformer.Transform);
        }

        public override IEnumerable<Row> Run() {
            Output = _stream.ToEnumerable();
            return Output;
        }
    }
}