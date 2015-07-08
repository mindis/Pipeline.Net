using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;
using Pipeline.Transformers;

namespace Pipeline.Validators {
    public class CompositeValidator : BaseTransform, ITransform {
        private readonly IEnumerable<ITransform> _transforms;
        private readonly Func<Row, object> _validate;

        public CompositeValidator(Process process, Entity entity, Field field, IEnumerable<ITransform> transforms, IPipelineLogger logger)
            : base(process, entity, field, field.GetDefaultOf<Transform>(t => { t.Method = "composite"; }), logger) {
            _transforms = transforms.ToArray();

            if (field.Type.StartsWith("bool")) {
                _validate = r => _transforms.All(t => (bool)t.Transform(r)[Field]);
            } else {
                _validate = r => string.Concat(_transforms.Select(t => t.Transform(r)[Field] + " ")).Trim();
            }

            Name = string.Format("composite validation on {0}, {1}", field.Alias, string.Join(",", _transforms.Cast<BaseTransform>().Select(t => t.Name)));
        }

        public Row Transform(Row row) {
            row[Field] = _validate(row);
            Increment();
            return row;
        }

        public Transform InterpretShorthand(string args, List<string> problems) {
            throw new NotImplementedException();
        }
    }
}