using Pipeline.Interfaces;

namespace Pipeline {
    public class NullInitializer : IAction {
        public ActionResponse Execute() {
            return new ActionResponse();
        }
    }
}