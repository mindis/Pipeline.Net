using System.Collections.Generic;
using System.Linq;

namespace Pipeline {
   public interface IScriptParser {
      IEnumerable<string> Parse(IScript script);
   }

}