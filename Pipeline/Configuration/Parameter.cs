using System;
using System.Linq;
using Cfg.Net;

namespace Pipeline.Configuration {

    public class Parameter : CfgNode {

        Field _loadedField;
        string _type;
        private string _field;
        private string _entity;

        [Cfg(value = "")]
        public string Entity {
            get { return _entity; }
            set {
                _entity = value;
                _loadedField = null;  //invalidate cache
            }
        }

        [Cfg(value = "")]
        public string Field {
            get { return _field; }
            set {
                _field = value;
                _loadedField = null; //invalidate cache
            }
        }

        [Cfg(value = "")]
        public string Name { get; set; }
        [Cfg(value = null)]
        public string Value { get; set; }
        [Cfg(value = true)]
        public bool Input { get; set; }

        [Cfg(value = "string", domain = Constants.TypeDomain, ignoreCase = true)]
        public string Type {
            get { return _type; }
            set { _type = value != null && value.StartsWith("sy", StringComparison.OrdinalIgnoreCase) ? value.ToLower().Replace("system.", string.Empty) : value; }
        }

        public bool HasValue() {
            return Value != null;
        }

        public bool IsField(Process process) {

            if (_loadedField != null)
                return true;

            if (string.IsNullOrEmpty(Entity)) {
                _loadedField = process.GetAllFields().FirstOrDefault(f => f.Alias == Field) ?? process.GetAllFields().FirstOrDefault(f => f.Name == Field);
                return _loadedField != null;
            }

            Entity entity;
            if (process.TryGetEntity(Entity, out entity)) {
                if (entity.TryGetField(Field, out _loadedField)) {
                    return true;
                }
            }
            return false;
        }

        public Field AsField(Process process) {
            if (_loadedField != null)
                return _loadedField;

            if (string.IsNullOrEmpty(Entity)) {
                _loadedField = process.GetAllFields().FirstOrDefault(f => f.Alias == Field) ?? process.GetAllFields().FirstOrDefault(f => f.Name == Field);
                return _loadedField;
            }

            Entity entity;
            if (process.TryGetEntity(Entity, out entity)) {
                if (entity.TryGetField(Field, out _loadedField)) {
                    return _loadedField;
                }
            }
            return null;
        }
    }

}