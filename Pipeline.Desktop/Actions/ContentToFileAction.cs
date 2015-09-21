using System.IO;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Desktop.Actions {
    public class ContentToFileAction : IAction {
        private readonly PipelineContext _context;
        private readonly Action _action;

        public ContentToFileAction(PipelineContext context, Action action) {
            _context = context;
            _action = action;
        }

        public ActionResponse Execute() {
            var to = new FileInfo(_action.To);
            if (string.IsNullOrEmpty(_action.Content)) {
                _context.Warn("Nothing to write to {0}", to.Name);
            } else {
                _context.Info("Writing content to {0}", to.Name);
                File.WriteAllText(to.FullName, _action.Content);
            }
            return new ActionResponse();
        }
    }
}
