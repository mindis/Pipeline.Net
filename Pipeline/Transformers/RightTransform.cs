using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline.Transformers {
   public class RightTransform : BaseTransform, ITransform {
      readonly Field _input;

      public RightTransform(PipelineContext context)
            : base(context) {
         _input = SingleInput();
      }

      public Row Transform(Row row) {
         row.SetString(Context.Field, row.GetString(_input).Right(Context.Transform.Length));
         Increment();
         return row;
      }

   }
}