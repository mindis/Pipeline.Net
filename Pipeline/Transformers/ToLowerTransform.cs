using Pipeline.Configuration;

namespace Pipeline.Transformers {
   public class ToLowerTransform : BaseTransform, ITransform {
      readonly Field _input;

      public ToLowerTransform(PipelineContext context) : base(context) {
         _input = SingleInput();
      }
      public Row Transform(Row row) {
         row.SetString(Context.Field, row.GetString(_input).ToLowerInvariant());
         Increment();
         return row;
      }
   }
}
