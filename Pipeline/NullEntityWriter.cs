using System.Collections.Generic;

namespace Pipeline {
   public class NullEntityWriter : IWrite {

      public int Write(IEnumerable<Row> rows) {
         return 0;
      }

      public void LoadVersion() {
      }

   }
}