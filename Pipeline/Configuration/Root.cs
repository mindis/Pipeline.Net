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
                params IDependency[] dependencies)
            : base(dependencies) {
            LoadShorthand(shorthand);
            Load(cfg);
        }

        public Root() {
        }

    }
}