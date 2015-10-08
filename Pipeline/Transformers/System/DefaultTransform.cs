using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Transformers.System {

    public class DefaultTransform : BaseTransform, ITransform {

        readonly Field[] _fields;
        readonly Dictionary<string, object> _typeDefaults;
        readonly Dictionary<int, Func<object>> _getDefaultFor = new Dictionary<int, Func<object>>();
        readonly Func<IField, short> _index;

        public DefaultTransform(PipelineContext context, IEnumerable<Field> fields)
            : base(context) {
            _fields = fields.ToArray();
            _typeDefaults = Constants.TypeDefaults();
            if (context.Entity.IsMaster) {
                _index = (f) => f.MasterIndex;
            } else {
                _index = (f) => f.Index;
            }
            foreach (var fld in _fields) {
                var f = fld;
                _getDefaultFor[_index(f)] = () => f.Default == Constants.DefaultSetting ? _typeDefaults[f.Type] : f.Convert(f.Default);
            }

        }

        public Row Transform(Row row) {
            foreach (var field in _fields.Where(f => row[f] == null)) {
                row[field] = _getDefaultFor[_index(field)]();
            }
            Increment();
            return row;
        }

    }
}