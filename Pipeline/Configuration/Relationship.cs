using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cfg.Net;

namespace Pipeline.Configuration {
    public class Relationship : CfgNode {

        public RelationshipSummary Summary { get; set; }

        public Relationship() {
            Summary = new RelationshipSummary();
        }

        [Cfg(value = "", required = true, unique = false)]
        public string LeftEntity { get; set; }

        [Cfg(value = "", required = true, unique = false)]
        public string RightEntity { get; set; }

        [Cfg(value = "")]
        public string LeftField { get; set; }

        [Cfg(value = "")]
        public string RightField { get; set; }

        [Cfg(value = false)]
        public bool Index { get; set; }

        [Cfg]
        public List<Join> Join { get; set; }

        public IEnumerable<string> GetLeftJoinFields() {
            return LeftField == string.Empty ? Join.Select(j => j.LeftField).ToArray() : new[] { LeftField };
        }

        public IEnumerable<string> GetRightJoinFields() {
            return RightField == string.Empty ? Join.Select(j => j.RightField).ToArray() : new[] { RightField };
        }

    }
}