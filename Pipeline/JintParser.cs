using System.Linq;
using Jint.Parser;
using System;
using Pipeline.Extensions;
using Pipeline.Interfaces;

namespace Pipeline {
   public class JintParser : IScriptParser {
      readonly JavaScriptParser _jint = new JavaScriptParser();
      readonly ParserOptions _options;

      public JintParser() {
         _options = new ParserOptions() { Tolerant = true };
      }

      public void Parse(IScript script, Action<string,object[]> error) {
         try {
            var program = _jint.Parse(script.Script, _options);
            if (program != null && program.Errors != null && program.Errors.Any()) {
               foreach (var e in program.Errors) {
                  var message = string.Format("{0}, script: {1}...", e.Message, script.Script.Left(30));
                  error(message, new object[0]);
               }
            }
         } catch (ParserException ex) {
            var message = string.Format("{0}, script: {1}...", ex.Message, script.Script.Left(30));
            error(message, new object[0]);
         }
      }
   }
}
