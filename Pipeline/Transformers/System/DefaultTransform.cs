using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers.System {

   public class DefaultTransform : BaseTransform, ITransform {

      readonly Field[] _fields;
      readonly Dictionary<string, object> _typeDefaults;
      readonly Dictionary<int, Func<object>> _getDefaultFor = new Dictionary<int, Func<object>>();
      readonly Func<IField, short> _index;

      public DefaultTransform(PipelineContext context)
          : base(context) {
         _fields = context.GetAllEntityFields().ToArray();
         _typeDefaults = Constants.TypeDefaults();
         if (context.Entity.IsMaster) {
            _index = (f) => { return f.MasterIndex; };
         } else {
            _index = (f) => { return f.Index; };
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