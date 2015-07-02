using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class Provider : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
        [Cfg(required = true)]
        public string Type { get; set; }
    }
}