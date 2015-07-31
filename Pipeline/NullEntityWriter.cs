using System.Collections.Generic;

namespace Pipeline {
   public class NullEntityWriter : IEntityWriter {

      public PipelineContext Context { get; private set; }

      public void Initialize() { }

      public NullEntityWriter(PipelineContext context) {
         Context = context;
      }

      public int Write(IEnumerable<Row> rows) {
         return 0;
      }

      public void LoadVersion() {
      }

   }
}