using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public abstract class BaseTransform {
        public PipelineContext Context { get; }

        protected BaseTransform(PipelineContext context) {
            context.Activity = PipelineActivity.Transform;
            Context = context;
            Context.Debug("Registered");
        }

        public long RowCount { get; set; }

        protected virtual void Increment() {
            RowCount++;
            if (RowCount % Context.Entity.LogInterval == 0) {
                Context.Info(RowCount.ToString());
            }
        }

        /// <summary>
        /// A transformer's input can be entity fields, process fields, or the field the transform is in.
        /// </summary>
        /// <returns></returns>
        List<Field> ParametersToFields() {

            var fields = Context.Transform.Parameters
                .Where(p => p.IsField(Context.Process))
                .Select(p => p.AsField(Context.Process))
                .ToList();

            if (!fields.Any()) {
                fields.Add(Context.Field);
            }
            return fields;
        }

        public Field SingleInput() {
            return ParametersToFields().First();
        }

        /// <summary>
        /// Only used with producers, see Transform.Producers()
        /// </summary>
        /// <returns></returns>
        public Field SingleInputForMultipleOutput() {
            if (Context.Transform.Parameter != string.Empty) {
                return Context.Entity == null
                    ? Context.Process.GetAllFields()
                        .First(f => f.Alias == Context.Transform.Parameter || f.Name == Context.Transform.Parameter)
                    : Context.Entity.GetAllFields()
                        .First(f => f.Alias == Context.Transform.Parameter || f.Name == Context.Transform.Parameter);
            }
            return Context.Field;
        }

        public Field[] MultipleInput() {
            return ParametersToFields().ToArray();
        }

        public Field[] MultipleOutput() {
            return ParametersToFields().ToArray();
        }

    }
}