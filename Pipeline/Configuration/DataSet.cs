using System.Collections.Generic;
using Cfg.Net;

namespace Pipeline.Configuration {
    public class DataSet : CfgNode {
        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg]
        public List<Dictionary<string, string>> Rows { get; set; }

    }
}