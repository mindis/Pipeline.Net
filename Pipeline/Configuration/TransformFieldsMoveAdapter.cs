using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline.Configuration {

    public class TransformFieldsMoveAdapter {

        private readonly Entity _entity;

        public TransformFieldsMoveAdapter(Entity entity) {
            _entity = entity;
        }

        public void Adapt(string transformName) {

            var fields = new Dictionary<string, Dictionary<string, List<Field>>>();
            var calculatedFields = new Dictionary<string, Dictionary<string, List<Field>>>();

            fields[_entity.Alias] = GetFields(_entity.Fields, transformName);
            RemoveFields(_entity.Fields, transformName);

            calculatedFields[_entity.Alias] = GetFields(_entity.CalculatedFields, transformName);
            RemoveFields(_entity.CalculatedFields, transformName);

            InsertAsCalculatedFields(fields);
            InsertAsCalculatedFields(calculatedFields);
        }

        public Dictionary<string, List<Field>> GetFields(List<Field> fields, string transformName) {
            var result = new Dictionary<string, List<Field>>();
            foreach (var field in fields) {
                foreach (var transform in field.Transforms) {
                    if (!transform.Method.Equals(transformName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    result[field.Alias] = new List<Field>();
                    foreach (var tField in transform.Fields) {
                        tField.Input = false;
                        result[field.Alias].Add(tField);
                    }
                }
            }
            return result;
        }

        public void RemoveFields(List<Field> fields, string transformName) {
            foreach (var field in fields) {
                foreach (var transform in field.Transforms) {
                    if (!transform.Method.Equals(transformName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    transform.Fields.Clear();
                }
            }
        }

        public void InsertAsCalculatedFields(Dictionary<string, Dictionary<string, List<Field>>> fields) {
            foreach (var pair in fields) {
                foreach (var field in pair.Value) {
                    var fieldElement = _entity.CalculatedFields.FirstOrDefault(f => f.Alias == field.Key);
                    var index = fieldElement == null ? 0 : _entity.CalculatedFields.IndexOf(fieldElement) + 1;
                    foreach (var element in field.Value) {
                        _entity.CalculatedFields.Insert(index, element);
                        index++;
                    }
                }
            }
        }

    }
}