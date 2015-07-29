using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
   public class FormatTransform : BaseTransform, ITransform {

      readonly Field[] _input;

      public FormatTransform(PipelineContext context)
          : base(context) {
         _input = MultipleInput();
      }

      public Row Transform(Row row) {
         row.SetString(Context.Field, string.Format(Context.Transform.Format, _input.Select(f => row.GetString(f)).ToArray()));
         Increment();
         return row;
      }

   }
}