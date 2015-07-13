using System.Collections.Generic;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class XmlDecodeTransform : HtmlDecodeTransform {

        public XmlDecodeTransform(PipelineContext context)
            : base(context) {
        }

        //public new static Transform InterpretShorthand(string args, List<string> problems) {
        //    return Parameterless("xmldecode", "xml-decoded", args, problems);
        //}
    }
}