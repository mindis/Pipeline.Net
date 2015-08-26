using Cfg.Net;

namespace Pipeline.Configuration {
    public class NameReference : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
    }
}