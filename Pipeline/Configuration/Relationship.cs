using System.Collections.Generic;
using System.Linq;
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

        protected override void PreValidate() {
            PreValidateNormalizeJoin();
        }

        private void PreValidateNormalizeJoin() {
            if (LeftField == string.Empty)
                return;
            Join.Insert(0, this.GetDefaultOf<Join>(j => {
                j.LeftField = LeftField;
                j.RightField = RightField;
            }));
            LeftField = string.Empty;
            RightField = string.Empty;
        }

        public IEnumerable<string> GetLeftJoinFields() {
            return Join.Select(j => j.LeftField).ToArray();
        }

        public IEnumerable<string> GetRightJoinFields() {
            return Join.Select(j => j.RightField).ToArray();
        }

    }
}