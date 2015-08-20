using Pipeline.Configuration;
using Pipeline.Interfaces;
using System;
using System.Collections.Generic;

namespace Pipeline.Transformers {
   public class MapTransform : BaseTransform, ITransform {
      readonly Field _input;
      readonly Dictionary<object, Func<Row, object>> _map = new Dictionary<object, Func<Row, object>>();
      const string CATCH_ALL = "*";

      public MapTransform(PipelineContext context, IMapReader mapReader) : base(context) {
         _input = SingleInput();
         foreach (var item in mapReader.Read(context)) {
            var from = _input.Convert(item.From);
            if (item.To == string.Empty) {
               var field = context.Entity.GetField(item.Parameter);
               _map[from] = (r) => r[field];
            } else {
               var to = context.Field.Convert(item.To);
               _map[from] = (r) => to;
            }
         }
         if (!_map.ContainsKey(CATCH_ALL)) {
            var value = context.Field.Convert(context.Field.Default);
            _map[CATCH_ALL] = (r) => value;
         }

      }
      public Row Transform(Row row) {
         row[Context.Field] = _map.ContainsKey(row[_input]) ? _map[row[_input]](row) : _map[CATCH_ALL](row);
         Increment();
         return row;
      }
   }

}
