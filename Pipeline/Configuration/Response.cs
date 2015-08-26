using System.Collections.Generic;
using Cfg.Net;

namespace Pipeline.Configuration {
    public class Response : CfgNode {

        [Cfg(value = "")]
        public string Request { get; set; }

        [Cfg(value = (short)200)]
        public short Status { get; set; }

        [Cfg(value = "OK")]
        public string Message { get; set; }

        [Cfg(value = (long)0)]
        public long Time { get; set; }

        [Cfg]
        public List<Empty> Content { get; set; }

        [Cfg]
        public List<Empty> Rows { get; set; }

        [Cfg]
        public List<Empty> Log { get; set; }
    }
}