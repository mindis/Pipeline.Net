using Cfg.Net;

namespace Pipeline.Configuration {
    public class MapItem : CfgNode {

        [Cfg(value = "", required = true, unique = true)]
        public string From { get; set; }

        [Cfg(value = "")]
        public string Parameter { get; set; }

        [Cfg(value = "")]
        public string To { get; set; }

    }
}