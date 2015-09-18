using System.Collections.Generic;

namespace Pipeline.Interfaces {

   public interface IWriteOutput {
      void Write(IEnumerable<Row> rows);
      void LoadVersion();
   }
}