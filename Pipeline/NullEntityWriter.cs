using System.Collections.Generic;

namespace Pipeline {
   public class NullEntityWriter : IWrite {

      public void Write(IEnumerable<Row> rows) {
      }

      public void LoadVersion() {
      }

   }
}