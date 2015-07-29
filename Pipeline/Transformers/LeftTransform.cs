using Pipeline.Extensions;

namespace Pipeline.Transformers {
   public class LeftTransform : BaseTransform, ITransform {

      readonly int _length;
      readonly IField _input;

      public LeftTransform(PipelineContext context)
            : base(context) {
         _length = context.Transform.Length;
         _input = SingleInput();
      }

      public Row Transform(Row row) {
         row.SetString(Context.Field, row.GetString(_input).Left(_length));
         Increment();
         return row;
      }

   }
}