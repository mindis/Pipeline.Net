using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
   public class BaseEntityWriter : BaseEntity {

      public Field[] OutputFields { get; set; }

      public BaseEntityWriter(PipelineContext context)
            : base(context) {
         OutputFields = context.GetAllEntityOutputFields().ToArray();
         Connection = context.Process.Connections.First(c => c.Name == "output");
      }
   }
}