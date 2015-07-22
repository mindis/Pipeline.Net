using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;

namespace Pipeline {
    public class DefaultPipeline : IEntityPipeline {

        private readonly IEntityController _controller;

        protected IEntityReader Reader { get; private set; }
        protected IEntityWriter Writer { get; private set; }
        protected IMasterUpdater MasterUpdater { get; private set; }
        protected List<ITransform> Transformers { get; private set; }

        public DefaultPipeline(IEntityController controller) {
            _controller = controller;
            Transformers = new List<ITransform>();
            _controller.Start();
        }

        public void Initialize() {
            _controller.Initialize();
        }

        public void Register(IEntityReader reader) {
            reader.Context.Activity = PipelineActivity.Extract;
            Reader = reader;
        }

        public void Register(ITransform transformer) {
            transformer.Context.Activity = PipelineActivity.Transform;
            Transformers.Add(transformer);
        }

        public void Register(IEntityWriter writer) {
            writer.Context.Activity = PipelineActivity.Load;
            Writer = writer;
        }

        public void Register(IMasterUpdater updater) {
            updater.Context.Activity = PipelineActivity.Load;
            MasterUpdater = updater;
        }

        public virtual IEnumerable<Row> Run() {
            Writer.LoadVersion();
            return Transformers.Aggregate(Reader.Read(), (current, transform) => current.Select(transform.Transform));
        }

        public void Execute() {
            Writer.Write(Run());
            MasterUpdater.Update();
            _controller.End();
        }

    }
}