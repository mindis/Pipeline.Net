using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;

namespace Pipeline {
    public class DefaultPipeline : IPipeline {
        private readonly IEntityController _controller;
        protected List<IEntityReader> Readers { get; private set; }
        protected IEntityWriter Writer { get; private set; }
        protected List<ITransform> Transformers { get; private set; }

        public DefaultPipeline(IEntityController controller) {
            _controller = controller;
            Readers = new List<IEntityReader>();
            Transformers = new List<ITransform>();
            _controller.Start();
        }

        public void Register(IEntityReader reader) {
            reader.Context.Activity = PipelineActivity.Extract;
            Readers.Add(reader);
        }

        public void Register(ITransform transformer) {
            transformer.Context.Activity = PipelineActivity.Transform;
            Transformers.Add(transformer);
        }

        public void Register(IEntityWriter writer) {
            writer.Context.Activity = PipelineActivity.Load;
            Writer = writer;
        }

        public virtual IEnumerable<Row> Run() {
            var min = Writer.GetVersion();
            var output = Readers.SelectMany(r => r.Read(min));
            return Transformers.Aggregate(output, (current, transform) => current.Select(transform.Transform));
        }

        public void Execute() {
            Writer.Write(Run(), _controller.BatchId);
            _controller.End();
        }

    }
}