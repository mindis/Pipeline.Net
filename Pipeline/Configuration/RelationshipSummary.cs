using System.Collections.Generic;
using System.Linq;

namespace Pipeline.Configuration {
    public class RelationshipSummary {

        public RelationshipSummary() {
            LeftFields = new List<Field>();
            RightFields = new List<Field>();
        }

        public Entity LeftEntity { get; set; }
        public List<Field> LeftFields { get; set; }

        public Entity RightEntity { get; set; }
        public List<Field> RightFields { get; set; }

        public bool IsAligned() {
            return LeftEntity != null && RightEntity != null && LeftFields.Any() && LeftFields.Count() == RightFields.Count();
        }

    }
}