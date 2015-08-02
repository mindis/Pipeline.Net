using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;

namespace Pipeline {
    public class DefaultPipeline : IEntityPipeline {

        readonly IEntityController _controller;

        protected IRead Reader { get; private set; }
        protected IWrite Writer { get; private set; }
        protected IUpdate MasterUpdater { get; private set; }
        protected List<ITransform> Transformers { get; private set; }

        public DefaultPipeline(IEntityController controller) {
            _controller = controller;
            Transformers = new List<ITransform>();
        }

        public void Initialize() {
            _controller.Initialize();
        }

        public void Register(IRead reader) {
            Reader = reader;
        }

        public void Register(ITransform transformer) {
            transformer.Context.Activity = PipelineActivity.Transform;
            Transformers.Add(transformer);
        }

        public void Register(IEnumerable<ITransform> transforms) {
            foreach (var transform in transforms) {
                Register(transform);
            }
        }

        public void Register(IWrite writer) {
            Writer = writer;
        }

        public void Register(IUpdate updater) {
            MasterUpdater = updater;
        }

        public virtual IEnumerable<Row> Run() {
            Writer.LoadVersion();
            return Transformers.Aggregate(Reader.Read(), (current, transform) => current.Select(transform.Transform));
        }

        public void Execute() {
            _controller.Start();
            Writer.Write(Run());
            MasterUpdater.Update();
            _controller.End();
        }

    }
}