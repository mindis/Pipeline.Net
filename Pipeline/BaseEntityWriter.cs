using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class BaseEntityWriter : BaseEntity {
        public Field[] OutputFields { get; set; }

        public BaseEntityWriter(PipelineContext context)
            : base(context) {
            OutputFields = context.Entity.GetAllFields().Where(f => f.Output).ToArray();
            Connection = context.Process.Connections.First(c => c.Name == "output");
        }
    }
}