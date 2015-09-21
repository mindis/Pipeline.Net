using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline {
    public class RenderTemplateAction : IAction {
        private readonly Template _template;
        private readonly ITemplateEngine _engine;

        public RenderTemplateAction(Template template, ITemplateEngine engine) {
            _template = template;
            _engine = engine;
        }

        public ActionResponse Execute() {
            var response = new ActionResponse(_engine.Render());
            foreach (var action in _template.Actions) {
                action.Content = response.Content;
            }
            return response;
        }
    }
}