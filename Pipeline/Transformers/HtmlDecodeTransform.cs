using System;
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

        public HtmlDecodeTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
            _builder = new StringBuilder();
        }

        public Row Transform(Row row) {
            row[Context.Field] = CfgNode.Decode(row[_input].ToString(), _builder);
            Increment();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            //return InterpretShorthand(args, problems);
            throw new NotImplementedException();
        }

        //public static Transform InterpretShorthand(string args, List<string> problems) {
        //    return Parameterless("htmldecode", "html-decoded", args, problems);
        //}
    }
}