using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class Relationship : CfgNode {

        [Cfg(value = "", required = true, unique = false)]
        public string LeftEntity { get; set; }
        [Cfg(value = "", required = true, unique = false)]
        public string RightEntity { get; set; }
        [Cfg(value = "")]
        public string LeftField { get; set; }
        [Cfg(value = "")]
        public string RightField { get; set; }
        [Cfg(value = false)]
        public bool Index { get; set; }

        [Cfg()]
        public List<Join> Join { get; set; }
    }
}