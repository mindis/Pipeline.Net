using System;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Test {
   public class ToStringTransform : BaseTransform, ITransform {
      Field _input;
      Func<object, string> _toString;

      public ToStringTransform(PipelineContext context) : base(context) {
         _input = SingleInput();
         if (context.Transform.Format == string.Empty) {
            _toString = (o) => o.ToString();
         } else {
            switch (_input.Type) {
               case "int32":
               case "int":
                  _toString = (o) => ((int)o).ToString(context.Transform.Format);
                  break;
               case "double":
                  _toString = (o) => ((double)o).ToString(context.Transform.Format);
                  break;
               case "short":
               case "int16":
                  _toString = (o) => ((short)o).ToString(context.Transform.Format);
                  break;
               case "long":
               case "int64":
                  _toString = (o) => ((long)o).ToString(context.Transform.Format);
                  break;
               case "datetime":
               case "date":
                  _toString = (o) => ((DateTime)o).ToString(context.Transform.Format);
                  break;
               default:
                  _toString = (o) => o.ToString();
                  break;
            }
         }
      }

      public Row Transform(Row row) {
         row[Context.Field] = _toString(row[_input]);
         Increment();
         return row;
      }
   }
}