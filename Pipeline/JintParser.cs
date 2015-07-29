using System.Collections.Generic;
using System.Linq;

namespace Pipeline{
   public class JintParser : IScriptParser {
      readonly Jint.Parser.JavaScriptParser _jint = new Jint.Parser.JavaScriptParser();
      public IEnumerable<string> Parse(IScript script) {
         var program = _jint.Parse(script.Script);
         if (program != null && program.Errors != null && program.Errors.Any()) {
            return program.Errors.Select(e => e.Message);
         }
         return new string[0];
      }
   }
}
