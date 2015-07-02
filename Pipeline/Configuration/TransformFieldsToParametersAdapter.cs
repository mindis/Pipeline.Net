using System;
using System.Collections.Generic;

namespace Pipeline.Configuration {

    public class TransformFieldsToParametersAdapter {
        private readonly Process _process;

        public TransformFieldsToParametersAdapter(Process process) {
            _process = process;
        }

        public int Adapt(string transformName) {
            var count = 0;
            foreach (Entity entity in _process.Entities) {
                count += AddParameters(entity.Fields, transformName, entity.Alias);
                count += AddParameters(entity.CalculatedFields, transformName, entity.Alias);
            }
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