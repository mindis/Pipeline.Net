using System;

namespace Pipeline.Interfaces {
   public interface IScriptParser {
      void Parse(IScript script, Action<string,object[]> error);
   }

}