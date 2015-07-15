using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class BaseEntityReader : BaseEntity {

        public int RowCapacity { get; set; }
        public Field[] InputFields { get; set; }

        public BaseEntityReader(PipelineContext context) :base(context) {
            RowCapacity = context.Entity.GetAllFields().Count();
            InputFields = context.Entity.Fields.Where(f => f.Input).ToArray();
            Connection = context.Process.Connections.First(c => c.Name == context.Entity.Connection);
        }

    }
}
