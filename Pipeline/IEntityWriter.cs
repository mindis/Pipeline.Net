using System.Collections.Generic;

namespace Pipeline {

   public interface IWrite {
      void Write(IEnumerable<Row> rows);
      void LoadVersion();
   }
}