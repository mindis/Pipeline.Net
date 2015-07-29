using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
   public class JoinTransform : BaseTransform, ITransform {
      readonly Field[] _input;

      public JoinTransform(PipelineContext context) : base(context) {
         _input = MultipleInput();
      }
      public Row Transform(Row row) {
         row.SetString(Context.Field, string.Join(Context.Transform.Separator, _input.Select(f => row.GetString(f))));
         Increment();
         return row;
      }
   }
}
