using System.Collections.Generic;

namespace Pipeline.Interfaces {

   public interface IWrite {
      void Write(IEnumerable<Row> rows);
      void LoadVersion();
   }
}