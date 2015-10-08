using Pipeline.Interfaces;

namespace Pipeline {
    public class NullUpdater : IUpdate {
        private readonly IContext _context;
        private readonly bool _log;

        public NullUpdater(IContext context, bool log = true) {
            _context = context;
            _log = log;
        }

        public void Update() {
            if (_log)
                _context.Info("Null Updating...");
        }
    }
}