using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class Environment : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
        [Cfg(required = true)]
        public List<Parameter> Parameters { get; set; }
        [Cfg()]
        public string Default { get; set; }
    }
}