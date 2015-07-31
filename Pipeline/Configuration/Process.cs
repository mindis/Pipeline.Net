using System;
using System.Collections.Generic;
using System.Linq;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {

   public class Process : CfgNode {

      const string ALL = "*";

      readonly Dictionary<string, string> _providerTypes = new Dictionary<string, string>();

      public Process() {

         _providerTypes.Add("sqlserver", "System.Data.SqlClient.SqlConnection, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
         _providerTypes.Add("mysql", "MySql.Data.MySqlClient.MySqlConnection, MySql.Data");
         _providerTypes.Add("postgresql", "Npgsql.NpgsqlConnection, Npgsql");
         var empties = new[] {
                "file",
                "folder",
                "internal",
                "console",
                "log",
                "elasticsearch",
                "solr",
                "lucene"
            };
         foreach (var empty in empties) {
            _providerTypes.Add(empty, empty);
         }
      }

      /// <summary>
      /// A name (of your choosing) to identify the process.
      /// </summary>
      [Cfg(value = "", required = true, unique = true)]
      public string Name { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// `True` by default.
      /// 
      /// Indicates the process is enabled.  The included executable (e.g. `tfl.exe`) 
      /// respects this setting and does not run the process if disabled (or `False`).
      /// </summary>
      [Cfg(value = true)]
      public bool Enabled { get; set; }

      /// <summary>
      /// Optional. 
      /// 
      /// A mode reflects the intent of running the process.
      ///  
      /// * `init` wipes everything out
      /// * <strong>`default`</strong> moves data through the pipeline, from input to output.
      /// 
      /// Aside from these, you may use any mode (of your choosing).  Then, you can control
      /// whether or not templates and/or actions run by setting their modes.
      /// </summary>
      [Cfg(value = "", toLower = true)]
      public string Mode { get; set; }

      /// <summary>
      /// Optional.  Default is `false`
      /// 
      /// If true, process entities in parallel.  If false, process them one by one in their configuration order.
      /// 
      /// Parallel *on* allows you to process all the entities at the same time, potentially faster.
      /// Parallel *off* allows you to have one entity depend on a previous entity's data.
      /// </summary>
      [Cfg(value = false)]
      public bool Parallel { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// A choice between `defer`, `linq`, `parallel.linq`, `streams`, `parallel.streams`.
      /// 
      /// The default `defer` defers this decision to the entity's Pipeline setting.
      /// </summary>
      [Cfg(value = "defer", domain = "defer,linq,streams,parallel.linq,parallel.streams,linq.optimizer,parallel.linq.optimizer", toLower = true)]
      public string Pipeline { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// If your output is a relational database that supports views and `StarEnabled` is `True`,
      /// this is the name of a view that projects fields from all the entities in the
      /// star-schema as a single flat projection.
      /// 
      /// If not set, it is the combination of the process name, and "Star." 
      /// </summary>
      [Cfg(value = "")]
      public string Star { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// `True` by default.
      /// 
      /// Star refers to star-schema Transformalize is creating.  You can turn this off 
      /// if your intention is not to create a star-schema.  A `False` setting here may
      /// speed things up.
      /// </summary>
      [Cfg(value = true)]
      public bool StarEnabled { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// Choices are `html` and <strong>`raw`</strong>.
      /// 
      /// This refers to the razor templating engine's content type.  If you're rendering HTML 
      /// markup, use `html`, if not, using `raw` may inprove performance.
      /// </summary>
      [Cfg(value = "raw", domain = "raw,html", toLower = true)]
      public string TemplateContentType { get; set; }

      /// <summary>
      /// Optional.
      /// 
      /// Indicates the data's time zone.
      /// 
      /// It is used as the `to-time-zone` setting for `now()` and `timezone()` transformations
      /// if the `to-time-zone` is not set.
      /// 
      /// NOTE: Normally, you should keep the dates in UTC until presented to the user. 
      /// Then, have the client application convert UTC to the user's time zone.
      /// </summary>
      [Cfg(value = "")]
      public string TimeZone { get; set; }

      /// <summary>
      /// Optional
      /// 
      /// If your output is a relational database that supports views, this is the name of
      /// a view that projects fields from all the entities.  This is different from 
      /// the Star view, as it's joins are exactly as configured in the <relationships/> 
      /// collection.
      /// 
      /// If not set, it is the combination of the process name, and "View." 
      /// </summary>
      [Cfg(value = "")]
      public string View { get; set; }

      [Cfg(value = false)]
      public bool ViewEnabled { get; set; }

      /// <summary>
      /// A collection of [Actions](/action)
      /// </summary>
      [Cfg()]
      public List<Action> Actions { get; set; }

      /// <summary>
      /// A collection of [Calculated Fields](/calculated-field)
      /// </summary>
      [Cfg()]
      public List<Field> CalculatedFields { get; set; }

      /// <summary>
      /// A collection of [Connections](/connection)
      /// </summary>
      [Cfg(required = false)]
      public List<Connection> Connections { get; set; }

      /// <summary>
      /// A collection of [Entities](/entity)
      /// </summary>
      [Cfg(required = true)]
      public List<Entity> Entities { get; set; }

      /// <summary>
      /// Settings to control [file inspection](/file-inspection).
      /// </summary>
      [Cfg()]
      public List<FileInspection> FileInspection { get; set; }

      /// <summary>
      /// A collection of [Logs](/log)
      /// </summary>
      [Cfg(sharedProperty = "rows", sharedValue = (long)10000)]
      public List<Log> Log { get; set; }

      /// <summary>
      /// A collection of [Maps](/map)
      /// </summary>
      [Cfg()]
      public List<Map> Maps { get; set; }

      /// <summary>
      /// A collection of [Relationships](/relationship)
      /// </summary>
      [Cfg()]
      public List<Relationship> Relationships { get; set; }

      /// <summary>
      /// A collection of [Scripts](/script)
      /// </summary>
      [Cfg(sharedProperty = "path", sharedValue = "")]
      public List<Script> Scripts { get; set; }

      /// <summary>
      /// A collection of [Search Types](/search-type)
      /// </summary>
      [Cfg()]
      public List<SearchType> SearchTypes { get; set; }

      /// <summary>
      /// A collection of [Templates](/template)
      /// </summary>
      [Cfg(sharedProperty = "path", sharedValue = "")]
      public List<Template> Templates { get; set; }

      /// <summary>
      /// A collection of [Data Sets](/data-sets)
      /// </summary>
      [Cfg]
      public List<DataSet> DataSets { get; set; }

      protected override void Modify() {

         if (string.IsNullOrEmpty(Star)) {
            Star = Name + "Star";
         }

         if (string.IsNullOrEmpty(View)) {
            View = Name + "View";
         }

         // calculated fields are not input
         foreach (var calculatedField in CalculatedFields) {
            calculatedField.Input = false;
         }

         ModifyDefaultConnection("input");
         ModifyDefaultConnection("output");
         ModifyDefaultEntityConnections();
         ModifyDefaultOutput();
         ModifyDefaultSearchTypes();

         try {
            ExpandShortHandTransforms();
         } catch (Exception ex) {
            Error("Trouble expanding short hand transforms. {0}", ex.Message);
         }

         ModifyMergeParameters();
         ModifyPrimaryKeyTypes();
      }

      void ModifyPrimaryKeyTypes() {
         // set primary on none
         foreach (var entity in Entities) {
            foreach (var field in entity.GetAllFields()) {
               if (field.PrimaryKey) {
                  field.KeyType = KeyType.Primary;
               }
            }
         }
      }

      void ModifyLogLimits() {
         var entitiesAny = Entities.Any();
         var fieldsAny = GetAllFields().Any(f => f.Transforms.Any());
         var transformsAny = GetAllTransforms().Any();

         LogLimit = Name.Length;
         EntityLogLimit = entitiesAny ? Entities.Select(e => e.Alias.Length).Max() : 0;
         FieldLogLimit = fieldsAny ? GetAllFields().Where(f => f.Transforms.Any()).Select(f => f.Alias.Length).Max() : 0;
         TransformLogLimit = transformsAny ? GetAllTransforms().Select(t => t.Method.Length).Max() : 0;
      }

      void ModifyKeys() {
         var counter = 0;
         Key = Name;
         // entities, fields, and transforms
         foreach (var entity in Entities) {
            entity.Key = Key + entity.Alias;
            foreach (var field in entity.GetAllFields()) {
               field.Key = entity.Key + field.Alias;
               foreach (var transform in field.Transforms) {
                  transform.Key = field.Key + transform.Method + counter++;
               }
            }
         }

         //calculated fields, and transforms
         foreach (var field in CalculatedFields) {
            field.Key = Key + field.Alias;
            foreach (var transform in field.Transforms) {
               transform.Key = field.Key + transform.Method + counter++;
            }
         }
      }

      /// <summary>
      /// Set by Process.ModifyKeys for keyed dependency injection
      /// </summary>
      public string Key { get; set; }

      /// <summary>
      /// Log limits, set by ModifyLogLimits
      /// </summary>
      public int LogLimit { get; set; }
      public int EntityLogLimit { get; set; }
      public int FieldLogLimit { get; set; }
      public int TransformLogLimit { get; set; }

      void ModifyDefaultEntityConnections() {
         foreach (var entity in Entities.Where(entity => !entity.HasConnection())) {
            entity.Connection = Connections.Any(c => c.Name == "input") ? "input" : Connections.First().Name;
         }
      }

      void ModifyDefaultConnection(string name) {
         if (Connections.All(c => c.Name != name)) {
            this.Connections.Add(this.GetDefaultOf<Connection>(c => { c.Name = name; }));
         }
      }

      void ModifyMergeParameters() {
         foreach (var entity in Entities) {
            entity.MergeParameters();
         }
         var index = 0;
         foreach (var field in CalculatedFields) {
            foreach (var transform in field.Transforms.Where(t => t.Parameter != string.Empty && !Transform.Producers().Contains(t.Method))) {
               if (transform.Parameter == ALL) {
                  foreach (var entity in Entities) {
                     foreach (var entityField in entity.GetAllFields().Where(f => f.Output)) {
                        transform.Parameters.Add(GetParameter(entity.Alias, entityField.Alias, entityField.Type));
                     }
                  }
                  var thisField = field;
                  foreach (var cf in CalculatedFields.Take(index).Where(cf => cf.Name != thisField.Name)) {
                     transform.Parameters.Add(GetParameter(string.Empty, cf.Alias, cf.Type));
                  }
               } else {
                  if (transform.Parameter.IndexOf('.') > 0) {
                     var split = transform.Parameter.Split(new[] { '.' });
                     transform.Parameters.Add(GetParameter(split[0], split[1]));
                  } else {
                     transform.Parameters.Add(GetParameter(transform.Parameter));
                  }
               }
               transform.Parameter = string.Empty;
            }
            index++;
         }
      }

      Parameter GetParameter(string field) {
         return GetDefaultOf<Parameter>(p => {
            p.Field = field;
         });
      }

      Parameter GetParameter(string entity, string field) {
         return GetDefaultOf<Parameter>(p => {
            p.Entity = entity;
            p.Field = field;
         });
      }

      Parameter GetParameter(string entity, string field, string type) {
         return GetDefaultOf<Parameter>(p => {
            p.Entity = entity;
            p.Field = field;
            p.Type = type;
         });
      }

      void ModifyDefaultSearchTypes() {

         if (SearchTypes.All(st => st.Name != "none"))
            SearchTypes.Add(this.GetDefaultOf<SearchType>(st => {
               st.Name = "none";
               st.MultiValued = false;
               st.Store = false;
               st.Index = false;
            }));

         if (SearchTypes.All(st => st.Name != "default"))
            SearchTypes.Add(this.GetDefaultOf<SearchType>(st => {
               st.Name = "default";
               st.MultiValued = false;
               st.Store = true;
               st.Index = true;
            }));
      }

      void ModifyDefaultOutput() {
         if (Connections.All(c => c.Name != "output"))
            Connections.Add(this.GetDefaultOf<Connection>(c => {
               c.Name = "output";
               c.Provider = "internal";
            }));
      }

      protected override void Validate() {
         ValidateDuplicateEntities();
         ValidateDuplicateFields();
         ValidateLogConnections();
         ValidateRelationships();
         ValidateEntityConnections();
         ValidateActionConnections();
         ValidateTemplateActionConnections();
         ValidateTransformConnections();
         ValidateMapConnections();
         ValidateMapTransforms();
         ValidateDataSets();

         if (Errors().Length == 0) {
            Entities.First().IsMaster = true;
            ModifyKeys();
            ModifyLogLimits();
            ModifyRelationshipToMaster();
            ModifyIndexes();
         }
      }

      void ValidateMapTransforms() {
         foreach (var transform in GetAllTransforms().Where(t => t.Method == "map")) {
            if (Maps.All(m => m.Name != transform.Map)) {
               Error("A map transform returns in an invalid map: {0}.", transform.Map);
            }
         }
      }

      void ModifyIndexes() {
         short entityIndex = -1;
         for (int i = 0; i < Entities.Count; i++) {
            short masterIndex = -1;
            var context = new PipelineContext(new Logging.NullLogger(), this, Entities[i]);
            context.Entity.Index = ++entityIndex;
            short fieldIndex = -1;
            short stringIndex = -1;
            foreach (var field in context.GetAllEntityFields()) {
               field.EntityIndex = context.Entity.Index;
               field.Index = ++fieldIndex;
               field.StringIndex = field.Is("string") ? ++stringIndex : (short)-1;
               if (context.Entity.IsMaster) {
                  field.MasterIndex = ++masterIndex;
               }
            }
         }
      }

      void ModifyRelationshipToMaster() {
         foreach (var entity in Entities) {
            entity.RelationshipToMaster = ReadRelationshipToMaster(entity);
         }
      }

      IEnumerable<Relationship> ReadRelationshipToMaster(Entity entity) {

         if (entity.IsMaster)
            return new List<Relationship>();

         var relationships = Relationships.Where(r => r.Summary.RightEntity.Equals(entity)).ToList();

         if (relationships.Any() && !relationships.Any(r => r.Summary.LeftEntity.IsMaster)) {
            var leftEntity = relationships.Last().Summary.LeftEntity;
            relationships.AddRange(ReadRelationshipToMaster(leftEntity));
         }
         return relationships;
      }

      void ValidateDataSets() {
         foreach (var dataSet in DataSets.Where(dataSet => Entities.All(e => e.Name != dataSet.Name))) {
            Error("The {0} data set does not match any entities (by name).", dataSet.Name);
         }
      }

      void ValidateMapConnections() {
         foreach (var map in Maps.Where(m => m.Query != string.Empty).Where(map => Connections.All(c => c.Name != map.Connection))) {
            Error("The {0} map references an invalid connection: {1}.", map.Name, map.Connection);
         }
      }

      IEnumerable<Transform> GetAllTransforms() {
         var transforms = Entities.SelectMany(entity => entity.GetAllTransforms()).ToList();
         transforms.AddRange(CalculatedFields.SelectMany(field => field.Transforms));
         return transforms;
      }


      void ValidateTransformConnections() {

         var methodsWithConnections = new[] { "mail", "run" };

         foreach (var transform in GetAllTransforms().Where(t => methodsWithConnections.Any(nc => nc == t.Method))) {
            var connection = Connections.FirstOrDefault(c => c.Name == transform.Connection);
            if (connection == null) {
               Error("The {0} transform references an invalid connection: {2}.", transform.Method, transform.Connection);
               continue;
            }

            switch (transform.Method) {
               case "mail":
                  if (connection.Provider != "mail") {
                     Error("The {0} transform references the wrong type of connection: {1}.", transform.Method, connection.Provider);
                  }
                  break;
            }
         }
      }

      void ValidateEntityConnections() {
         foreach (var entity in Entities.Where(entity => Connections.All(c => c.Name != entity.Connection))) {
            Error("The {0} entity references an invalid connection: {1}.", entity.Name, entity.Connection);
         }
      }

      void ValidateTemplateActionConnections() {
         foreach (var action in Templates.SelectMany(template => template.Actions.Where(a => a.Connection != string.Empty).Where(action => Connections.All(c => c.Name != action.Connection)))) {
            Error("The {0} template action references an invalid connection: {1}.", action.Name, action.Connection);
         }
      }

      void ValidateActionConnections() {
         foreach (var action in Actions.Where(action => action.Connection != string.Empty).Where(action => Connections.All(c => c.Name != action.Connection))) {
            Error("The {0} action references an invalid connection: {1}.", action.Name, action.Connection);
         }
      }

      void ValidateRelationships() {
         // count check
         if (Entities.Count > 1 && Relationships.Count + 1 < Entities.Count) {
            Error("You have {0} entities so you need {1} relationships. You have {2} relationships.", Entities.Count, Entities.Count - 1, Relationships.Count);
         }

         //entity alias, name check, and if that passes, do field alias, name check
         foreach (var relationship in Relationships) {
            var error = false;

            // validate (and modify) left side
            Entity leftEntity;
            if (TryGetEntity(relationship.LeftEntity, out leftEntity)) {
               relationship.Summary.LeftEntity = leftEntity;
               foreach (var leftField in relationship.GetLeftJoinFields()) {
                  Field field;
                  if (leftEntity.TryGetField(leftField, out field)) {
                     relationship.Summary.LeftFields.Add(field);
                  } else {
                     Error("A relationship references a left-field that doesn't exist: {0}", leftField);
                     error = true;
                  }
               }
            } else {
               Error("A relationship references a left-entity that doesn't exist: {0}", relationship.LeftEntity);
               error = true;
            }

            //validate (and modify) right side
            Entity rightEntity;
            if (TryGetEntity(relationship.RightEntity, out rightEntity)) {
               relationship.Summary.RightEntity = rightEntity;
               foreach (var rightField in relationship.GetRightJoinFields()) {
                  Field field;
                  if (rightEntity.TryGetField(rightField, out field)) {
                     relationship.Summary.RightFields.Add(field);
                  } else {
                     Error("A relationship references a right-field that doesn't exist: {0}", rightField);
                     error = true;
                  }
               }
            } else {
               Error("A relationship references a right-entity that doesn't exist: {0}", relationship.RightEntity);
               error = true;
            }

            //if everything is cool, set the foreign key flags
            if (!error && relationship.Summary.IsAligned()) {
               for (int i = 0; i < relationship.Summary.LeftFields.Count; i++) {
                  var leftField = relationship.Summary.LeftFields[i];
                  leftField.KeyType |= KeyType.Foreign;
                  if (!leftField.Output) {
                     Warn("Foreign key {0} on left side must be output. Overriding output to true.", leftField.Alias);
                     leftField.Output = true;
                  }
               }
            }

         }

      }

      public Entity GetEntity(string nameOrAlias) {
         var entity = Entities.FirstOrDefault(e => e.Alias == nameOrAlias);
         return entity ?? Entities.FirstOrDefault(e => e.Name != e.Alias && e.Name == nameOrAlias);
      }

      public bool TryGetEntity(string nameOrAlias, out Entity entity) {
         entity = GetEntity(nameOrAlias);
         return entity != null;
      }

      void ValidateLogConnections() {
         if (Log.Count <= 0)
            return;

         foreach (var log in Log.Where(log => log.Connection != Constants.DefaultSetting).Where(log => Connections.All(c => c.Name != log.Connection))) {
            Error(string.Format("Log {0}'s connection {1} doesn't exist.", log.Name, log.Connection));
         }
      }

      void ValidateDuplicateFields() {
         var fieldDuplicates = Entities
             .SelectMany(e => e.GetAllFields())
             .Where(f => !f.PrimaryKey && f.Alias != "TflHashCode")
             .Union(CalculatedFields)
             .GroupBy(f => f.Alias)
             .Where(group => @group.Count() > 1)
             .Select(group => @group.Key)
             .ToArray();
         foreach (var duplicate in fieldDuplicates) {
            Error(
                string.Format(
                    "The entity field '{0}' occurs more than once. Remove, alias, or prefix one.",
                    duplicate));
         }
      }

      void ValidateDuplicateEntities() {
         var entityDuplicates = Entities
             .GroupBy(e => e.Alias)
             .Where(group => @group.Count() > 1)
             .Select(group => @group.Key)
             .ToArray();
         foreach (var duplicate in entityDuplicates) {
            Error(string.Format("The '{0}' entity occurs more than once. Remove or alias one.", duplicate));
         }
      }

      /// <summary>
      /// Converts custom shorthand transforms
      /// </summary>
      public void ExpandShortHandTransforms() {
         foreach (var entity in Entities) {
            foreach (var field in entity.Fields) {
               field.ExpandShortHandTransforms();
            }
            foreach (var field in entity.CalculatedFields) {
               field.ExpandShortHandTransforms();
            }
         }
         foreach (var field in CalculatedFields) {
            field.ExpandShortHandTransforms();
         }
      }

      public IEnumerable<Field> GetAllFields() {
         var fields = new List<Field>();
         foreach (var e in Entities) {
            fields.AddRange(e.GetAllFields());
         }
         fields.AddRange(CalculatedFields);
         return fields;
      }

      public IEnumerable<Field> GetRelationshipFields(Entity entity) {

         //var relationships = Relationships.Where(r => r.Summary.LeftEntity.Alias != entity.Alias && r.Summary.RightEntity.Alias != entity.Alias).ToArray();

         var fields = new List<Field>();
         foreach (var r in Relationships) {
            var leftSide = r.Summary.LeftEntity.RelationshipToMaster.Count();
            var rightSide = r.Summary.RightEntity.RelationshipToMaster.Count();
            if (leftSide <= rightSide) {
               if (r.Summary.LeftEntity.Alias != entity.Alias && r.Summary.RightEntity.Alias != entity.Alias) {
                  foreach (var field in r.Summary.LeftFields) {
                     fields.Add(field);
                  }
               }
               fields.AddRange(r.Summary.RightEntity.Fields.Where(f => f.Output && f.Denormalize));
            } else {
               if (r.Summary.LeftEntity.Alias != entity.Alias && r.Summary.RightEntity.Alias != entity.Alias) {
                  foreach (var field in r.Summary.RightFields) {
                     fields.Add(field);
                  }
               }
               fields.AddRange(r.Summary.LeftEntity.Fields.Where(f => f.Output && f.Denormalize));
            }
         }

         return fields.Distinct();

      }

   }
}