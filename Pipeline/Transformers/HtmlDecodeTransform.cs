using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipeline.Configuration;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Transformers {
    /// <summary>
    /// Default (portable) XML,HTML Decode borrows from Cfg-Net
    /// </summary>
    public class HtmlDecodeTransform : BaseTransform, ITransform {
        private readonly Field _input;
        private readonly StringBuilder _builder;

        public HtmlDecodeTransform(Process process, Entity entity, Field field, Transform transform)
            : base(process, entity, field) {
            _input = ParametersToFields(transform).First();
            _builder = new StringBuilder();
            Name = "htmldecode";
        }

        public Row Transform(Row row) {
            row[Field] = CfgNode.Decode(row[_input].ToString(), _builder);
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            return Parameterless("htmldecode", "html-decoded", args, problems);
        }
    }
}