using System.Collections.Generic;

namespace Pipeline {

   public interface IEntityWriter {
      int Write(IEnumerable<Row> rows);
      void LoadVersion();
      PipelineContext Context { get; }
   }
}