using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public abstract class BaseTransform {
        private long _rowCount;

        public PipelineContext Context { get; private set; }

        protected BaseTransform(PipelineContext context) {
            Context = context;
        }

        public long RowCount {
            get { return _rowCount; }
            set { _rowCount = value; }
        }

        protected virtual void Increment() {
            _rowCount++;
            if (_rowCount % Context.Entity.LogInterval == 0) {
                Context.Info(_rowCount.ToString());
            }
        }

        /// <summary>
        /// A transformer's input can be entity fields, process fields, or the field the transform is in.
        /// </summary>
        /// <returns></returns>
        private List<Field> ParametersToFields() {

            var fields = Context.Transform.Parameters
                .Where(p => p.Field != string.Empty)
                .Select(p =>
                    Context.Entity == null ?
                    Context.Process.GetAllFields().First(f => f.Alias == p.Field || f.Name == p.Field) :
                    Context.Entity.GetAllFields().First(f => f.Alias == p.Field || f.Name == p.Field)
                ).ToList();

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