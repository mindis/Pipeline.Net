using System.Collections.Generic;
using Pipeline.Configuration;

namespace Pipeline.Shorthand {
    public interface IShorthandParser {
        Transform Parse(string args, List<string> problems);
    }
}