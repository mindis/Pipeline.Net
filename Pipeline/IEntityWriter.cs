using System.Collections.Generic;

namespace Pipeline {

   public interface IWrite {
      int Write(IEnumerable<Row> rows);
      void LoadVersion();
   }
}