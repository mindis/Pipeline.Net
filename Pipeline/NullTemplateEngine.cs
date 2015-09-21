using Pipeline.Interfaces;

namespace Pipeline {
    public class NullTemplateEngine : ITemplateEngine {
        public string Render() {
            return string.Empty;
        }
    }
}