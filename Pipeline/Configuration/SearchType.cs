using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class SearchType : CfgNode {
        [Cfg(value = "", required = true, unique = true)]
        public string Name { get; set; }
        [Cfg(value = true)]
        public bool Store { get; set; }
        [Cfg(value = true)]
        public bool Index { get; set; }
        [Cfg(value = false)]
        public bool MultiValued { get; set; }
        [Cfg(value = "")]
        public string Analyzer { get; set; }
        [Cfg(value = true)]
        public bool Norms { get; set; }
    }
}