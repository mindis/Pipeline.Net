using Cfg.Net;

namespace Pipeline.Configuration {
    public class Join : CfgNode {

        [Cfg( /* name= "left-field" */ value = "", required = true)]
        public string LeftField { get; set; }
        [Cfg( /* name= "right-field" */ value = "", required = true)]
        public string RightField { get; set; }

    }
}