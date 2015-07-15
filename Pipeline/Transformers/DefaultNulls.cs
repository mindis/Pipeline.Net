using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public class DefaultNulls : BaseTransform, ITransform {

        private readonly Field[] _fields;
        private readonly Dictionary<string, object> _typeDefaults;
        private readonly Dictionary<int, Func<object>> _getDefaultFor = new Dictionary<int, Func<object>>();

        public DefaultNulls(PipelineContext context)
            : base(context) {
            _fields = context.Entity.GetAllFields().ToArray();
            _typeDefaults = Constants.TypeDefaults();
            foreach (var fld in _fields) {
                var f = fld;
                _getDefaultFor[f.Index] = () => f.Default == Constants.DefaultSetting ? _typeDefaults[f.Type] : f.Convert(f.Default);
            }
        }

        public Row Transform(Row row) {
            foreach (var field in _fields.Where(f => row[f] == null)) {
                row[field] = _getDefaultFor[field.Index]();
            }
            Increment();
            return row;
        }

    }
}