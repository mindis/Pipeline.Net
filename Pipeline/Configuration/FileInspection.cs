using System.Collections.Generic;
using Cfg.Net;

namespace Pipeline.Configuration {

    public class FileInspection : CfgNode {

        [Cfg(value = (short)0 )]
        public short MaxLength { get; set; }
        [Cfg(value = (short)0)]
        public short MinLength { get; set; }
        [Cfg(value = "", required = true, unique = true)]
        public string Name { get; set; }
        [Cfg(value = (short)100)]
        public short Sample { get; set; }
        [Cfg(value=false)]
        public bool IgnoreEmpty { get; set; }
        [Cfg(value = 100)]
        public int LineLimit { get; set; }

        [Cfg(required = false)]
        public List<TflType> Types { get; set; }
        [Cfg(required = true)]
        public List<Delimiter> Delimiters { get; set; }
    }
}