using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Shorthand {

    public class ParameterlessParser : BaseShorthandParser, IShorthandParser {
        private readonly string _method;

        public ParameterlessParser(string method) {
            _method = method;
        }

        public Transform Parse(string args, List<string> problems) {
            var result = new ParameterlessValidator(_method).Validate(args);
            if (!result.IsValid) {
                problems.AddRange(result.Errors.Select(e => e.ErrorMessage));
                return Guard();
            }

            return DefaultConfiguration(t => {
                t.Method = _method;
                t.IsShortHand = true;
            });
        }
    }
}