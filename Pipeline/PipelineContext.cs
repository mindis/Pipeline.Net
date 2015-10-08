using System;
using Pipeline.Configuration;
using Pipeline.Extensions;
using Pipeline.Logging;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Pipeline.Interfaces;

namespace Pipeline {
    public class PipelineContext : IContext {
        PipelineActivity _activity;
        public Process Process { get; set; }
        public Entity Entity { get; set; }
        public Field Field { get; set; }
        public Transform Transform { get; set; }
        public object[] ForLog { get; }
        public IPipelineLogger Logger { get; set; }

        public PipelineActivity Activity {
            get { return _activity; }
            set {
                _activity = value;
                ForLog[2] = value.ToString().Left(1);
            }
        }

        public PipelineContext(
            IPipelineLogger logger,
            Process process,
            Entity entity = null,
            Field field = null,
            Transform transform = null
        ) {
            ForLog = new object[5];
            Logger = logger;
            Activity = PipelineActivity.Transform;
            Process = process;
            Entity = entity ?? process.GetValidatedOf<Entity>(e => { e.Name = string.Empty; });
            Field = field ?? process.GetValidatedOf<Field>(f => { f.Name = string.Empty; });
            Transform = transform ?? process.GetDefaultOf<Transform>(t => { t.Method = string.Empty; });
            ForLog[0] = process.Name.PadRight(process.LogLimit, ' ').Left(process.LogLimit);
            ForLog[1] = Entity.Alias.PadRight(process.EntityLogLimit, ' ').Left(process.EntityLogLimit);
            ForLog[2] = ' ';
            ForLog[3] = Field.Alias.PadRight(process.FieldLogLimit, ' ').Left(process.FieldLogLimit);
            ForLog[4] = Transform.Method.PadRight(process.TransformLogLimit, ' ').Left(process.TransformLogLimit);
        }

        public void Info(string message, params object[] args) {
            Logger.Info(this, message, args);
        }

        public void Warn(string message, params object[] args) {
            Logger.Warn(this, message, args);
        }

        public void Debug(string message, params object[] args) {
            Logger.Debug(this, message, args);
        }

        public void Error(string message, params object[] args) {
            Logger.Error(this, message, args);
        }

        public void Error(Exception exception, string message, params object[] args) {
            Logger.Error(this, exception, message, args);
        }

        public IEnumerable<Field> GetAllEntityOutputFields() {
            return GetAllEntityFields().Where(f => f.Output);
        }

        /// <summary>
        /// Gets all fields for an entity.  Takes into account 
        /// the master entity's responsibility for carrying
        /// the process' calculated fields and 
        /// related entity's denormalized fields.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Field> GetAllEntityFields() {
            if (!Entity.IsMaster) {
                return Entity.GetAllFields();
            }

            var fields = new List<Field>();
            fields.AddRange(Entity.GetAllFields());
            fields.AddRange(Process.CalculatedFields);
            fields.AddRange(Process.GetRelationshipFields(Entity));
            return fields;
        }

    }
}