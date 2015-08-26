using Cfg.Net;

namespace Pipeline.Configuration {
    public class Script : CfgNode {

        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
        
        [Cfg(required = true)]
        public string File { get; set; }
        
        [Cfg(value="")]
        public string Path { get; set; }

        [Cfg(value="")]
        public string Body { get; set; }

    }
}