using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;
using Pipeline.Transformers.System;
using Pipeline.Interfaces;

namespace Pipeline {
    public class DefaultPipeline : IEntityPipeline {

        readonly IEntityController _controller;

        protected IReadInput Reader { get; private set; }
        protected IWriteOutput Writer { get; private set; }
        protected IUpdate MasterUpdater { get; private set; }
        protected List<ITransform> Transformers { get; }

        readonly PipelineContext _context;

        public DefaultPipeline(IEntityController controller, IContext context) {
            _context = (PipelineContext)context;
            _controller = controller;
            Transformers = new List<ITransform> {
                new DefaultTransform(_context),
                new TflHashCodeTransform(_context)
            };
        }

        public void Initialize() {
            _controller.Initialize();
        }

        public void Register(IReadInput reader) {
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

        public void Register(IWriteOutput writer) {
            Writer = writer;
        }

        public void Register(IUpdate updater) {
            MasterUpdater = updater;
        }

        public virtual IEnumerable<Row> Run() {
            Reader.LoadVersion();
            Writer.LoadVersion();
            if (_context.Entity.NeedsUpdate()) {
                _context.Info("data change? Yes");
                Transformers.Add(new StringTruncateTransfom(_context));
                return Transformers.Aggregate(Reader.Read(), (current, transform) => current.Select(transform.Transform));
            }
            _context.Info("data change? No");
            return Enumerable.Empty<Row>();
        }

        public void Execute() {
            _controller.Start();
            Writer.Write(Run());
            MasterUpdater.Update();
            _controller.End();
        }

    }
}