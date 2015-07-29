using System;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {

   public class ConcatTransform : BaseTransform, ITransform {
      readonly Field[] _input;

      public ConcatTransform(PipelineContext context)
          : base(context) {
         _input = MultipleInput();
      }

      public Row Transform(Row row) {
         row.SetString(Context.Field, string.Concat(_input.Select(f => row.GetString(f))));
         Increment();
         return row;
      }

   }
}