using Pipeline.Interfaces;

namespace Pipeline {
    public class NullAction : IAction {
        public ActionResponse Execute() {
            return new ActionResponse();
        }
    }
}