using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Loggers;

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
                Dictionary<string, string> parameters = null,
                ILogger logger = null)
            : base(null, logger) {

            Load(xml, parameters);
        }

        public Root() {
        }
    }
}