using System.IO;
using Pipeline.Interfaces;
using Action = Pipeline.Configuration.Action;

namespace Pipeline.Desktop.Actions {
    public class FileToFileAction : IAction {
        private readonly PipelineContext _context;
        private readonly Action _action;

        public FileToFileAction(PipelineContext context, Action action) {
            _context = context;
            _action = action;
        }

        public ActionResponse Execute() {
            var from = new FileInfo(_action.From);
            var to = new FileInfo(_action.To);
            _context.Info("Copying {0} to {1}", from.Name, to.Name);
            File.Copy(from.FullName, to.FullName, true);
            return new ActionResponse();
        }
    }
}