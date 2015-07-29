using System;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Validators {
   public class ContainsValidater : BaseTransform, ITransform {

      readonly Field _input;
      readonly Func<string, object> _contains;

      public ContainsValidater(PipelineContext context)
            : base(context) {

         _input = SingleInput();

         if (context.Field.Type.StartsWith("bool", StringComparison.Ordinal)) {
            _contains = s => s.Contains(context.Transform.Value);
         } else {
            _contains = s => s.Contains(context.Transform.Value) ? String.Empty : String.Format("{0} does not contain {1}.", _input.Alias, context.Transform.Value);
         }

      }

      public Row Transform(Row row) {
         row[Context.Field] = _contains(row.GetString(_input));
         Increment();
         return row;
      }

   }
}