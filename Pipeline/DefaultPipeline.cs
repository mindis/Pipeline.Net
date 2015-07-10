using System.Collections.Generic;
using System.Linq;
using Pipeline.Logging;
using Pipeline.Transformers;

namespace Pipeline {
    public class DefaultPipeline : IPipeline {

        protected IPipelineLogger Logger { get; private set; }
        protected List<IEntityReader> Readers { get; private set; }
        protected List<ITransform> Transformers { get; private set; }

        public DefaultPipeline(IPipelineLogger logger) {
            Readers = new List<IEntityReader>();
            Transformers = new List<ITransform>();
            Logger = logger;
        }

        public void Register(IEntityReader reader) {
            reader.Logger = Logger;
            reader.Context.Activity = PipelineActivity.Extract;
            Readers.Add(reader);
        }

        public void Register(ITransform transformer) {
            transformer.Logger = Logger;
            transformer.Context.Activity = PipelineActivity.Transform;
            Transformers.Add(transformer);
        }

        public virtual IEnumerable<Row> Run() {
            var output = Readers.SelectMany(r => r.Read());
            return Transformers.Aggregate(output, (current, transform) => current.Select(transform.Transform));
        }
    }
}