using Cfg.Net;

namespace Pipeline.Configuration {
    public class InputOutput : CfgNode {

        [Cfg(value = "", required = true)]
        public string Connection { get; set; }
        [Cfg(value = "")]
        public string Name { get; set; }
        [Cfg(value = "")]
        public string RunField { get; set; }
        [Cfg(value = "Equal", domain = Constants.ComparisonDomain)]
        public string RunOperator { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string RunType { get; set; }
        [Cfg(value = "")]
        public string RunValue { get; set; }

    }
}