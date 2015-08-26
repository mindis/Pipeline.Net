using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;

namespace Pipeline.Configuration {
    public class Entity : CfgNode {

        public bool IsMaster { get; set; }

        [Cfg(required = false, unique = true)]
        public string Alias { get; set; }

        [Cfg(value = "", toLower = true)]
        public string Connection { get; set; }

        [Cfg(value = false)]
        public bool Delete { get; set; }

        public bool ShouldUpdateMaster() {
            if (IsMaster)
                return false;
            if (IsFirstRun())
                return false;
            return Fields.Any(f => f.KeyType.HasFlag(KeyType.Foreign) || f.Denormalize);
        }

        public Field[] GetPrimaryKey() {
            return GetAllFields().Where(f => f.PrimaryKey).ToArray();
        }

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

        public Field TflHashCode() {
            return CalculatedFields.First(f => f.Name == Constants.TflHashCode);
        }

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
        [Cfg(value = (long)10000)]
        public long LogInterval { get; set; }

        /// <summary>
        /// Set by Process.ModifyKeys for keyed dependency injection
        /// </summary>
        public string Key { get; set; }

        public IEnumerable<Relationship> RelationshipToMaster { get; internal set; }

        public int BatchId { get; set; }
        public object MinVersion { get; set; }
        public object MaxVersion { get; set; }

        public bool NeedsUpdate() {
            if (MinVersion == null)
                return true;
            if (MaxVersion == null)
                return true;

            var field = GetVersionField();
            if(field.Type == "byte[]") {
                var beginBytes = (byte[])MinVersion;
                var endBytes = (byte[])MaxVersion;
                return !beginBytes.SequenceEqual(endBytes);
            } else {
                return !MinVersion.Equals(MaxVersion);
            }
        }

        public short Index { get; internal set; }

        public IEnumerable<Field> GetAllFields() {
            var fields = new List<Field>();
            foreach (var f in Fields) {
                fields.Add(f);
                fields.AddRange(f.Transforms.SelectMany(transform => transform.Fields));
            }
            fields.AddRange(CalculatedFields);
            return fields;
        }

        public IEnumerable<Field> GetAllOutputFields() {
            return GetAllFields().Where(f => f.Output);
        }

        protected override void PreValidate() {
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

            try {
                AdaptFieldsCreatedFromTransforms();
            } catch (Exception ex) {
                Error("Trouble adapting fields created from transforms. {0}", ex.Message);
            }

            ModifyTflHashCode();
            ModifyMissingPrimaryKey();

        }

        void ModifyTflHashCode() {
            var hash = GetDefaultOf<Field>(f => {
                f.Name = Constants.TflHashCode;
                f.Alias = Constants.TflHashCode;
                f.Type = "int";
                f.Input = false;
                f.Output = true;
            });
            CalculatedFields.Add(hash);
        }


        /// <summary>
        /// Adds a primary key if there isn't one.
        /// </summary>
        void ModifyMissingPrimaryKey() {

            if (!Fields.Any())
                return;

            if (Fields.Any(f => f.PrimaryKey))
                return;

            if (CalculatedFields.Any(cf => cf.PrimaryKey))
                return;

            CalculatedFields.First(cf => cf.Name == Constants.TflHashCode).PrimaryKey = true;
        }

        protected override void Validate() {
            var fields = GetAllFields().ToArray();
            var names = new HashSet<string>(fields.Select(f => f.Name).Distinct());
            var aliases = new HashSet<string>(fields.Select(f => f.Alias));

            ValidateVersion(names, aliases);
            ValidateFilter(names, aliases);
        }

        void ValidateVersion(ICollection<string> names, ICollection<string> aliases) {
            if (Version == string.Empty)
                return;

            if (names.Contains(Version))
                return;

            if (aliases.Contains(Version))
                return;

            Error("Cant't find version field '{0}' in entity '{1}'", Version, Name);
        }

        void ValidateFilter(ICollection<string> names, ICollection<string> aliases) {
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

        Parameter GetParameter(string entity, string field, string type) {
            return GetDefaultOf<Parameter>(p => {
                p.Entity = entity;
                p.Field = field;
                p.Type = type;
            });
        }

        Parameter GetParameter(string entity, string field) {
            return GetDefaultOf<Parameter>(p => {
                p.Entity = entity;
                p.Field = field;
            });
        }

        public bool HasConnection() {
            return Connection != string.Empty || Input.Count > 0;
        }

        void AdaptFieldsCreatedFromTransforms() {
            foreach (var method in Transform.Producers()) {
                while (new TransformFieldsToParametersAdapter(this).Adapt(method) > 0) {
                    new TransformFieldsMoveAdapter(this).Adapt(method);
                }
            }
        }

        public override string ToString() {
            return Alias;
        }

        string OutputName(string processName) {
            return (PrependProcessNameToOutputName ? processName + Alias : Alias);
        }

        public Field GetVersionField() {
            return GetAllFields().FirstOrDefault(f => f.Alias == Version || f.Name == Version);
        }

        public Field GetField(string aliasOrName) {
            return GetAllFields().FirstOrDefault(f => f.Alias == aliasOrName) ?? GetAllFields().FirstOrDefault(f => f.Name == aliasOrName);
        }

        public bool TryGetField(string aliasOrName, out Field field) {
            field = GetField(aliasOrName);
            return field != null;
        }
        public bool IsFirstRun() {
            return MinVersion == null;
        }

        public string OutputTableName(string processName) {
            return OutputName(processName) + "Table";
        }

        public string OutputViewName(string processName) {
            return OutputName(processName);
        }

        public string GetExcelName() {
            return Constants.GetExcelName(Index);
        }

        public int Inserts { get; set; }
        public int Updates { get; set; }
        public int Deletes { get; set; }

        [Cfg(value=0)]
        public int ReadSize { get; set; }

        [Cfg(value =250)]
        public int InsertSize { get; set; }

        [Cfg(value = 50)]
        public int UpdateSize { get; set; }

        [Cfg(value ="default", domain ="init,default", toLower = true)]
        public string Mode { get; set; }

    }
}