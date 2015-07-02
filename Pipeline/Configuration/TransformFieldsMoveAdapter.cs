using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline.Configuration {

    public class TransformFieldsMoveAdapter {

        private readonly Process _process;

        public TransformFieldsMoveAdapter(Process process) {
            _process = process;
        }

        public void Adapt(string transformName) {

            var fields = new Dictionary<string, Dictionary<string, List<Field>>>();
            var calculatedFields = new Dictionary<string, Dictionary<string, List<Field>>>();

            foreach (Entity entity in _process.Entities) {
                fields[entity.Alias] = GetFields(entity.Fields, transformName);
                RemoveFields(entity.Fields, transformName);

                calculatedFields[entity.Alias] = GetFields(entity.CalculatedFields, transformName);
                RemoveFields(entity.CalculatedFields, transformName);
            }

            InsertFields(fields);
            InsertFields(calculatedFields);
        }

        public Dictionary<string, List<Field>> GetFields(List<Field> fields, string transformName) {
            var result = new Dictionary<string, List<Field>>();
            foreach (Field field in fields) {
                foreach (var transform in field.Transforms) {
                    if (!transform.Method.Equals(transformName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    result[field.Alias] = new List<Field>();
                    foreach (Field tField in transform.Fields) {
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

        public void InsertFields(Dictionary<string, Dictionary<string, List<Field>>> fields) {
            foreach (var entity in fields) {
                foreach (var field in entity.Value) {
                    var entityElement = _process.Entities.First(e => e.Alias == entity.Key);
                    var fieldElement = entityElement.Fields.First(f => f.Alias == field.Key);
                    var index = entityElement.Fields.IndexOf(fieldElement) + 1;
                    foreach (var element in field.Value) {
                        entityElement.Fields.Insert(index, element);
                        index++;
                    }
                }
            }
        }

    }
}