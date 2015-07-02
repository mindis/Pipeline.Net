using System;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {

    public class Parameter : CfgNode {
        private string _type;

        [Cfg(value = "")]
        public string Entity { get; set; }
        [Cfg(value = "")]
        public string Field { get; set; }
        [Cfg(value = "")]
        public string Name { get; set; }
        [Cfg(value = null)]
        public string Value { get; set; }
        [Cfg(value = true)]
        public bool Input { get; set; }

        [Cfg(value = "string", domain = Constants.TypeDomain, ignoreCase = true)]
        public string Type {
            get { return _type; }
            set { _type = value != null && value.StartsWith("sy", StringComparison.OrdinalIgnoreCase) ? value.ToLower().Replace("system.", string.Empty) : value; }
        }

        public bool HasValue() {
            return Value != null;
        }
    }

}