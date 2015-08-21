using Pipeline.Interfaces;
using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class Root : CfgNode {

        [Cfg(sharedProperty = "default", sharedValue = "")]
        public List<Environment> Environments { get; set; }
        [Cfg(required = true)]
        public List<Process> Processes { get; set; }
        [Cfg]
        public List<Response> Response { get; set; }

        public Root(
                string xml,
                string shorthand,
                IValidator javascriptParser,
                Dictionary<string, string> parameters = null)
            : base(validators: new Dictionary<string, IValidator>() { { "js", javascriptParser } }) {
            LoadShorthand(shorthand);
            Load(xml, parameters);
        }

        public Root() {
        }

    }
}