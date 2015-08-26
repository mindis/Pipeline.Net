using Cfg.Net;

namespace Pipeline.Configuration {
    public class TflType : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Type { get; set; }
    }
}