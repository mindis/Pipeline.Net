using System.Collections.Generic;
using System.Linq;
using Cfg.Net;

namespace Pipeline.Configuration {
    public class Action : CfgNode {

        [Cfg(required = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(value = true)]
        public bool After { get; set; }
        [Cfg(value = "")]
        public string Arguments { get; set; }
        [Cfg(value = "")]
        public string Bcc { get; set; }
        [Cfg(value = false)]
        public bool Before { get; set; }
        [Cfg(value = "")]
        public string Body { get; set; }
        [Cfg(value = "")]
        public string Cc { get; set; }
        [Cfg(value = "")]
        public string Command { get; set; }
        [Cfg(value = "", toLower = true)]
        public string Connection { get; set; }
        [Cfg(value = "")]
        public string File { get; set; }
        [Cfg(value = "")]
        public string From { get; set; }
        [Cfg(value = true)]
        public bool Html { get; set; }
        [Cfg(value = "get", domain = "get,post", toLower = true, ignoreCase = true)]
        public string Method { get; set; }

        [Cfg(value = "*", toLower = true)]
        public string Mode { get; set; }

        [Cfg(value = "")]
        public string NewValue { get; set; }
        [Cfg(value = "")]
        public string OldValue { get; set; }
        [Cfg(value = "")]
        public string Subject { get; set; }
        [Cfg(value = 0)]
        public int TimeOut { get; set; }
        [Cfg(value = "")]
        public string To { get; set; }
        [Cfg(value = "")]
        public string Url { get; set; }

        public bool InTemplate { get; set; }
        public string Content { get; set; }

        [Cfg]
        public List<NameReference> Modes { get; set; }

        public string[] GetModes() {
            return Modes.Any() ? Modes.Select(m => m.Name).ToArray() : new[] { Mode };
        }

        protected override void Validate() {
            if (Before && After) {
                Error("The {0} action is set to run before AND after.  Please choose before OR after.", Name);
            }
        }

        protected override void PreValidate() {
            if (Name != null && Name == "copy" && File != string.Empty && To == string.Empty) {
                To = File;
            }
        }

        /// <summary>
        /// Set for dependency injection
        /// </summary>
        public string Key { get; set; }
    }
}