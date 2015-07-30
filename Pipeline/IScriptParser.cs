using System;

namespace Pipeline {
   public interface IScriptParser {
      void Parse(IScript script, Action<string,object[]> error);
   }

}