using System.Collections.Generic;
using Cfg.Net;

namespace Pipeline.Configuration {
    public class Environment : CfgNode {

        [Cfg(required = true, unique = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public List<Parameter> Parameters { get; set; }

    }
}