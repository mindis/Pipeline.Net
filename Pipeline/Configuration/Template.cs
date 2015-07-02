using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {

    public class Template : CfgNode {

        [Cfg( required = true, unique = true)]
        public string Name { get; set; }
        [Cfg( value = "raw", domain = "raw,html")]
        public string ContentType { get; set; }
        [Cfg( required = true, unique = true)]
        public string File { get; set; }
        [Cfg( value = false)]
        public bool Cache { get; set; }
        [Cfg( value = true)]
        public bool Enabled { get; set; }

        [Cfg( value = "razor", domain = "razor,velocity", toLower = true)]
        public string Engine { get; set; }
        [Cfg()]
        public string Path { get; set; }

        [Cfg()]
        public List<Parameter> Parameters { get; set; }
        [Cfg()]
        public List<Action> Actions { get; set; }

    }
}