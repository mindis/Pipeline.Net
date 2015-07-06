using System.Collections.Generic;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class XmlDecodeTransform : HtmlDecodeTransform {
        public XmlDecodeTransform(Process process, Entity entity, Field field, Transform transform)
            : base(process, entity, field, transform) {
            Name = "xmldecode";
        }
        public new static Transform InterpretShorthand(string args, List<string> problems) {
            return Parameterless("xmldecode", "xml-decoded", args, problems);
        }
    }
}