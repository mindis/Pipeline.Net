using System.Collections.Generic;
using Cfg.Net;
using Cfg.Net.Contracts;

namespace Pipeline.Configuration {
    public class Root : CfgNode {

        [Cfg(sharedProperty = "default", sharedValue = "")]
        public List<Environment> Environments { get; set; }
        [Cfg(required = true)]
        public List<Process> Processes { get; set; }
        [Cfg]
        public List<Response> Response { get; set; }

        public Root(
                string cfg,
                string shorthand,
                IValidator javascriptParser,
                IReader configurationReader = null,
                Dictionary<string, string> parameters = null)
            : base(
                  reader: configurationReader,
                  validators: new Dictionary<string, IValidator>() { { "js", javascriptParser } }
            ) {
            LoadShorthand(shorthand);
            Load(cfg, parameters);
        }

        public Root() {
        }

    }
}