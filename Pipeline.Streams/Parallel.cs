using System.Collections.Generic;
using Nessos.Streams;
using Nessos.Streams.CSharp;
using Pipeline.Logging;
using Pipeline.Transformers;

namespace Pipeline.Streams {

    public class Parallel : DefaultPipeline {

        private ParStream<Row> _stream;

        public Parallel(IPipelineLogger logger)
            : base(logger) {
        }

        public override void Input(IEnumerable<Row> input) {
            _stream = input.AsParStream();
        }

        public override void Register(ITransform transformer) {
            _stream = _stream.Select(transformer.Transform);
        }

        public override IEnumerable<Row> Run() {
            Output = _stream.Stream().ToEnumerable();
            return Output;
        }
    }
}