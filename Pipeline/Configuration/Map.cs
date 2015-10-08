using System.Collections.Generic;
using Cfg.Net;

namespace Pipeline.Configuration {

    public class Map : CfgNode {

        [Cfg(value = "input", toLower = true)]
        public string Connection { get; set; }
        [Cfg(required = true, unique = true, toLower =true)]
        public string Name { get; set; }
        [Cfg(value = "")]
        public string Query { get; set; }

        [Cfg(required = false)]
        public List<MapItem> Items { get; set; }

        protected override void Validate() {
            if (Items.Count == 0 && Query == string.Empty) {
                Error(string.Format("Map '{0}' needs items or a query.", Name));
            }
        }

        public string Key { get; set; }
    }
}