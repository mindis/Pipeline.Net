using Pipeline.Interfaces;
using System.Collections.Generic;

namespace Pipeline {
   public class NullEntityWriter : IWriteOutput {

      public void Write(IEnumerable<Row> rows) {
      }

      public void LoadVersion() {
      }

   }
}