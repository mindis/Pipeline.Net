using Pipeline.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class Root : CfgNode {
        IScriptParser _javascriptParser;

        [Cfg(sharedProperty = "default", sharedValue = "")]
        public List<Environment> Environments { get; set; }
        [Cfg(required = true)]
        public List<Process> Processes { get; set; }
        [Cfg]
        public List<Response> Response { get; set; }

        public Root(
                string xml,
                string shorthand,
                IScriptParser javaScriptParser = null,
                Dictionary<string, string> parameters = null)
            : base(null, null) {
            _javascriptParser = javaScriptParser;
            LoadShorthand(shorthand);
            Load(xml, parameters);
        }

        public Root() {
        }

        protected override void Validate() {
            ValidateJavascript();
        }

        void ValidateJavascript() {
            if (_javascriptParser != null) {
                foreach (var process in Processes) {
                    foreach (var field in process.GetAllFields()) {
                        foreach (var transform in field.Transforms.Where(t => t.Method == "javascript")) {
                            _javascriptParser.Parse(transform, Error);
                        }
                    }
                }
            }
        }
    }
}