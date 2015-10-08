using System.Linq;
using Jint.Parser;
using Pipeline.Extensions;
using Cfg.Net;
using Cfg.Net.Contracts;

namespace Pipeline {
    public class JintParser : IValidator {
        readonly JavaScriptParser _jint = new JavaScriptParser();
        readonly ParserOptions _options;

        public JintParser() {
            _options = new ParserOptions() { Tolerant = true };
        }

        public ValidatorResult Validate(string name, object value) {
            var result = new ValidatorResult();

            if (value == null)
                return result;

            var script = value.ToString();

            if (script == string.Empty)
                return result;

            try {
                var program = _jint.Parse(script, _options);
                if (program != null && program.Errors != null && program.Errors.Any()) {
                    result.Valid = false;
                    foreach (var e in program.Errors) {
                        result.Error("{0}, script: {1}...", e.Message, script.Left(30));
                    }
                }
            } catch (ParserException ex) {
                result.Valid = false;
                result.Error("{0}, script: {1}...", ex.Message, script.Left(30));
            }
            return result;
        }

    }
}
