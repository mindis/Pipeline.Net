using System;
using System.Collections.Generic;

namespace Pipeline.Configuration {

    public class TransformFieldsToParametersAdapter {
        readonly Entity _entity;

        public TransformFieldsToParametersAdapter(Entity entity) {
            _entity = entity;
        }

        public int Adapt(string transformName) {
            var count = 0;
            count += AddParameters(_entity.Fields, transformName, _entity.Alias);
            count += AddParameters(_entity.CalculatedFields, transformName, _entity.Alias);
            return count;
        }

        public int AddParameters(List<Field> fields, string transformName, string entity) {
            var count = 0;
            foreach (var field in fields) {
                foreach (var transform in field.Transforms) {
                    if (!transform.Method.Equals(transformName, StringComparison.OrdinalIgnoreCase)) continue;

                    for (var i = 0; i < transform.Fields.Count; i++) {
                        var tField = transform.Fields[i];
                        transform.Parameters.Add(tField.GetDefaultOf<Parameter>(p => {
                            p.Entity = entity;
                            p.Field = tField.Alias;
                            p.Name = tField.Name;
                            p.Input = false;
                        }));
                        count++;
                    }
                }
            }
            return count;
        }
    }
}