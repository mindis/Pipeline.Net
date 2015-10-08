using System.Collections.Generic;
using Cfg.Net;

namespace Pipeline.Configuration {

    public class Template : CfgNode {

        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
        [Cfg(value = "raw", domain = "raw,html", toLower = true, ignoreCase = true)]
        public string ContentType { get; set; }
        [Cfg(required = true, unique = true)]
        public string File { get; set; }
        [Cfg(value = false)]
        public bool Cache { get; set; }
        [Cfg(value = true)]
        public bool Enabled { get; set; }

        [Cfg(value = "razor", domain = "razor,velocity", toLower = true)]
        public string Engine { get; set; }

        [Cfg]
        public List<Parameter> Parameters { get; set; }
        [Cfg]
        public List<Action> Actions { get; set; }

        public string Key { get; set; }

        protected override void PostValidate() {
            var index = 0;
            foreach (var action in Actions) {
                action.InTemplate = true;
                action.Key = Name + (++index);
            }
        }
    }
}