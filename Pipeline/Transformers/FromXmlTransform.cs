using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Transformalize.Libs.Cfg.Net.Parsers.nanoXML;

namespace Pipeline.Transformers {


    /// <summary>
    /// The default (portable) fromxml transform, based on Cfg-Net's modified copy of NanoXml.
    /// </summary>
    public class FromXmlTransform : BaseTransform, ITransform {

        private readonly Field _input;
        private const StringComparison IC = StringComparison.OrdinalIgnoreCase;
        private readonly Dictionary<string, Field> _attributes = new Dictionary<string, Field>();
        private readonly Dictionary<string, Field> _elements = new Dictionary<string, Field>();
        private readonly bool _searchAttributes;
        private readonly int _total;

        public FromXmlTransform(Process process, Entity entity, Field field, Transform transform)
            : base(process, entity, field, transform) {
            _input = field;
            var output = ParametersToFields().ToArray();

            foreach (var f in output) {
                if (f.NodeType.Equals("attribute", IC)) {
                    _attributes[f.Name] = f;
                } else {
                    _elements[f.Name] = f;
                }
            }

            _searchAttributes = _attributes.Count > 0;
            _total = _elements.Count + _attributes.Count;

            Name = string.Format("fromxml({0}=>{1})", field.Name, string.Join(",", output.Select(f => f.Name)));
        }

        public Row Transform(Row row) {
            var xml = row[_input].ToString();
            if (xml.Equals(string.Empty)) {
                return row;
            }

            var count = 0;
            var doc = new NanoXmlDocument(xml);
            if (_elements.ContainsKey(doc.RootNode.Name)) {
                var field = _elements[doc.RootNode.Name];
                row[field] = field.Convert(doc.RootNode.Value ?? (field.ReadInnerXml ? doc.RootNode.InnerText() : doc.RootNode.ToString()));
                count++;
            }

            var subNodes = doc.RootNode.SubNodes.ToArray();
            while (subNodes.Any()) {
                var nextNodes = new List<NanoXmlNode>();
                foreach (var node in subNodes) {
                    if (_elements.ContainsKey(node.Name)) {
                        var field = _elements[node.Name];
                        count++;
                        var value = node.Value ?? (field.ReadInnerXml ? node.InnerText() : node.ToString());
                        if (!string.IsNullOrEmpty(value)) {
                            row[field] = field.Convert(value);
                        }
                    }
                    if (_searchAttributes) {
                        foreach (var attribute in node.Attributes.Where(attribute => _attributes.ContainsKey(attribute.Name))) {
                            var field = _attributes[attribute.Name];
                            count++;
                            if (!string.IsNullOrEmpty(attribute.Value)) {
                                row[field] = field.Convert(attribute.Value);
                            }
                        }
                    }
                    if (count < _total) {
                        nextNodes.AddRange(node.SubNodes);
                    }
                }
                subNodes = nextNodes.ToArray();
            }
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            return InterpretShorthand(args, problems);
        }

        public static Transform InterpretShorthand(string args, List<string> problems) {
            problems.Add("Sorry.  The fromxml transform is not supported in short-hand.  Use verbose configuration.");
            return Guard();
        }
    }
}