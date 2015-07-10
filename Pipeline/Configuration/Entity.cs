using System;
using System.Collections.Generic;
using System.Linq;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class Entity : CfgNode {

        [Cfg(required = false, unique = true)]
        public string Alias { get; set; }

        [Cfg(value = "", toLower = true)]
        public string Connection { get; set; }

        [Cfg(value = false)]
        public bool Delete { get; set; }

        /// <summary>
        /// Optional : `True` by default.
        /// 
        /// Currently this is a confusing option.  It's ambiguous:
        /// 
        /// * Does it mean "detect changes" between the input and output?
        ///   * If true, TFL will attempt to insert or update data, if a version field is available.  
        ///   * If false, TFL will only insert data, it will not compare input with output to `insert` or `update`.
        /// 
        /// * Does it affect what is loaded from the input
        ///   * If true, and input is capable of querying, and output has previous version value, TFL will pull delta from the input
        ///   * If false, TFL will not attempt to pull delta from input.
        /// 
        /// ###Ideas
        /// 
        /// * Add CanQuery to connection (true or false).  It's all queryable, just a matter of whether or not you have to load everything into memory.
        /// * This was mostly added to deal with importing single files.  If file connection was implemented to detect changes, might not need this. 
        /// * There are two concepts, querying just the delta from the input, and comparing the input and output, which requires a version and loading the corresponding output keys and version
        /// 
        /// </summary>
        [Cfg(value = true)]
        public bool DetectChanges { get; set; }

        [Cfg(value = false)]
        public bool Group { get; set; }
        [Cfg(value = "", required = true)]
        public string Name { get; set; }
        [Cfg(value = false)]
        public bool NoLock { get; set; }

        /// <summary>
        /// Optional.  Defaults to `linq`.
        /// 
        /// A choice between `linq`, `parallel.linq`, `streams`, `parallel.streams`.
        /// 
        /// **Note**: You can set each entity if you want, or control all entities from the Process' pipeline attribute.
        /// 
        /// In general, you should develop using `linq`, and once everything is stable, switch over to `parallel.linq`.
        /// </summary>
        [Cfg(value = "linq", domain = "linq,streams,parallel.linq,parallel.streams,linq.optimizer,parallel.linq.optimizer", toLower = true)]
        public string Pipeline { get; set; }

        [Cfg(value = "")]
        public string Prefix { get; set; }
        [Cfg(value = true)]
        public bool PrependProcessNameToOutputName { get; set; }
        [Cfg(value = "")]
        public string Query { get; set; }
        [Cfg(value = "")]
        public string QueryKeys { get; set; }
        [Cfg(value = 100)]
        public int Sample { get; set; }
        [Cfg(value = "")]
        public string Schema { get; set; }
        [Cfg(value = "")]
        public string Script { get; set; }
        [Cfg(value = "")]
        public string ScriptKeys { get; set; }
        [Cfg(value = false)]
        public bool TrimAll { get; set; }
        [Cfg(value = true)]
        public bool Unicode { get; set; }
        [Cfg(value = true)]
        public bool VariableLength { get; set; }
        [Cfg(value = "")]
        public string Version { get; set; }

        [Cfg(required = false)]
        public List<Filter> Filter { get; set; }
        [Cfg(required = false)]
        public List<Field> Fields { get; set; }
        [Cfg(required = false)]
        public List<Field> CalculatedFields { get; set; }
        [Cfg(required = false)]
        public List<InputOutput> Input { get; set; }
        [Cfg(required = false)]
        public List<InputOutput> Output { get; set; }

        /// <summary>
        /// Set by Process.ModifyKeys for keyed dependency injection
        /// </summary>
        public string Key { get; set; }

        [Cfg(value = (long)10000)]
        public long LogInterval { get; set; }

        public IEnumerable<Field> GetAllFields() {
            var fields = new List<Field>();
            foreach (var f in Fields) {
                fields.Add(f);
                fields.AddRange(f.Transforms.SelectMany(transform => transform.Fields));
            }
            fields.AddRange(CalculatedFields);
            return fields;
        }

        protected override void Modify() {
            if (string.IsNullOrEmpty(Alias)) {
                Alias = Name;
            }
            foreach (var calculatedField in CalculatedFields) {
                calculatedField.Input = false;
            }
            if (!string.IsNullOrEmpty(Prefix)) {
                foreach (var field in Fields.Where(f => f.Alias == f.Name)) {
                    field.Alias = Prefix + field.Name;
                }
            }
            ModifyMissingPrimaryKey();

            try {
                AdaptFieldsCreatedFromTransforms();
            } catch (Exception ex) {
                Error("Trouble adapting fields created from transforms. {0}", ex.Message);
            }

            ModifyFieldIndexes();
        }

        private void ModifyFieldIndexes() {
            var index = 0;
            foreach (var field in GetAllFields()) {
                field.Index = index++;
            }
        }

        /// <summary>
        /// Adds a primary key if there isn't one.
        /// </summary>
        private void ModifyMissingPrimaryKey() {

            if (!Fields.Any())
                return;

            if (Fields.Any(f => f.PrimaryKey))
                return;

            if (CalculatedFields.Any(cf => cf.PrimaryKey))
                return;

            if (!CalculatedFields.Any(cf => cf.Name.Equals("TflHashCode", StringComparison.OrdinalIgnoreCase))) {
                var pk = GetDefaultOf<Field>(f => {
                    f.Name = "TflHashCode";
                    f.Type = "int";
                    f.PrimaryKey = true;
                    f.T = "copy(*).concat().hashcode()";
                });

                CalculatedFields.Add(pk);
            }

            if (string.IsNullOrEmpty(Version)) {
                Version = "TflHashCode";
            }
        }

        protected override void Validate() {
            var fields = GetAllFields().ToArray();
            var names = new HashSet<string>(fields.Select(f => f.Name).Distinct());
            var aliases = new HashSet<string>(fields.Select(f => f.Alias));

            ValidateVersion(names, aliases);
            ValidateFilter(names, aliases);
        }

        private void ValidateVersion(ICollection<string> names, ICollection<string> aliases) {
            if (Version == string.Empty)
                return;

            if (names.Contains(Version))
                return;

            if (aliases.Contains(Version))
                return;

            Error("Cant't find version field '{0}' in entity '{1}'", Version, Name);
        }

        private void ValidateFilter(ICollection<string> names, ICollection<string> aliases) {
            if (Filter.Count == 0)
                return;

            foreach (var f in Filter) {
                if (f.Expression != string.Empty)
                    return;

                if (names.Contains(f.Left))
                    continue;

                if (aliases.Contains(f.Left))
                    continue;

                Error("A filter's left attribute must reference a defined field. '{0}' is not defined.", f.Left);
            }
        }

        public IEnumerable<Transform> GetAllTransforms() {
            var transforms = Fields.SelectMany(field => field.Transforms).ToList();
            transforms.AddRange(CalculatedFields.SelectMany(field => field.Transforms));
            return transforms;
        }

        public void MergeParameters() {

            foreach (var field in Fields) {
                foreach (var transform in field.Transforms.Where(t => t.Parameter != string.Empty && !Transform.Producers().Contains(t.Method))) {
                    if (transform.Parameter == "*") {
                        Error("You can not reference all parameters within an entity's field: {0}", field.Name);
                    } else {
                        transform.Parameters.Add(GetParameter(Alias, transform.Parameter));
                    }
                    transform.Parameter = string.Empty;
                }
            }

            var index = 0;
            foreach (var calculatedField in CalculatedFields) {
                foreach (var transform in calculatedField.Transforms.Where(t => t.Parameter != string.Empty && !Transform.Producers().Contains(t.Method))) {
                    if (transform.Parameter == "*") {
                        foreach (var field in Fields) {
                            transform.Parameters.Add(GetParameter(Alias, field.Alias, field.Type));
                        }
                        var thisField = calculatedField.Name;
                        foreach (var calcField in CalculatedFields.Take(index).Where(cf => cf.Name != thisField)) {
                            transform.Parameters.Add(GetParameter(Alias, calcField.Alias, calcField.Type));
                        }
                    } else {
                        transform.Parameters.Add(GetParameter(Alias, transform.Parameter));
                    }
                    transform.Parameter = string.Empty;
                }
                index++;
            }

        }

        private Parameter GetParameter(string entity, string field, string type) {
            return GetDefaultOf<Parameter>(p => {
                p.Entity = entity;
                p.Field = field;
                p.Type = type;
            });
        }

        private Parameter GetParameter(string entity, string field) {
            return GetDefaultOf<Parameter>(p => {
                p.Entity = entity;
                p.Field = field;
            });
        }

        public bool HasConnection() {
            return Connection != string.Empty || Input.Count > 0;
        }

        private void AdaptFieldsCreatedFromTransforms() {
            foreach (var method in Transform.Producers()) {
                while (new TransformFieldsToParametersAdapter(this).Adapt(method) > 0) {
                    new TransformFieldsMoveAdapter(this).Adapt(method);
                }
            }
        }

        public override string ToString() {
            return Alias;
        }

    }
}