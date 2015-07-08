using System.Collections.Generic;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Transformers {
    public class XmlDecodeTransform : HtmlDecodeTransform {

        public XmlDecodeTransform(Process process, Entity entity, Field field, Transform transform, IPipelineLogger logger)
            : base(process, entity, field, transform, logger) {
            Name = "xmldecode";
        }

        public new static Transform InterpretShorthand(string args, List<string> problems) {
            return Parameterless("xmldecode", "xml-decoded", args, problems);
        }
    }
}