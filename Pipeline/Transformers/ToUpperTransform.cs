using Pipeline.Configuration;

namespace Pipeline.Transformers {
   public class ToUpperTransform : BaseTransform, ITransform {
      readonly Field _input;

      public ToUpperTransform(PipelineContext context) : base(context) {
         _input = SingleInput();
      }
      public Row Transform(Row row) {
         row.SetString(Context.Field, row.GetString(_input).ToUpperInvariant());
         Increment();
         return row;
      }
   }
      
}
