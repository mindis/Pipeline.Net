using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;
using Pipeline.Interfaces;

namespace Pipeline {
    public class DefaultPipeline : IPipeline {

        readonly IEntityController _controller;

        protected IRead Reader { get; private set; }
        protected IWrite Writer { get; private set; }
        protected IUpdate Updater { get; private set; }
        protected List<ITransform> Transformers { get; }

        readonly PipelineContext _context;

        public DefaultPipeline(IEntityController controller, IContext context) {
            _context = (PipelineContext)context;
            _controller = controller;
            Transformers = new List<ITransform>();
        }

        public void Initialize() {
            _controller.Initialize();
        }

        public void Register(IRead reader) {
            Reader = reader;
        }

        public void Register(ITransform transform) {
            Transformers.Add(transform);
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
            Updater = updater;
        }

        public virtual IEnumerable<Row> Run() {
            if (_context.Entity.NeedsUpdate()) {
                _context.Info("data change? Yes");
                return Transformers.Aggregate(Reader.Read(), (current, transform) => current.Select(transform.Transform));
            }
            _context.Info("data change? No");
            return Enumerable.Empty<Row>();
        }

        public void Execute() {
            _controller.Start();
            Writer.Write(Run());
            Updater.Update();
            _controller.End();
        }

    }
}