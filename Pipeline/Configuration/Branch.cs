using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {

    public class Branch : CfgNode {

        [Cfg(unique = true)]
        public string Name { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string RunField { get; set; }
        [Cfg(value = "Equal", domain = Constants.ValidComparisons)]
        public string RunOperator { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string RunType { get; set; }
        [Cfg(value = "")]
        public string RunValue { get; set; }

        [Cfg()]
        public List<Transform> Transforms { get; set; }

    }
}