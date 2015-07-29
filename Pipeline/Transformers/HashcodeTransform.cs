using System.Linq;
using Pipeline.Configuration;
using System.Collections.Generic;

namespace Pipeline.Transformers {

   public class HashcodeTransform : BaseTransform, ITransform {

      readonly Field[] _input;
      public HashcodeTransform(PipelineContext context)
            : base(context) {
         _input = MultipleInput();
      }

      public Row Transform(Row row) {
         row[Context.Field] = GetHashCode(_input.Select(f => row[f]));
         Increment();
         return row;
      }

      public static int GetHashCode(params object[] args) {
         unchecked {
            int hash = 17;
            foreach (var obj in args) {
               hash = hash * 23 + obj.GetHashCode();
            }
            return hash;
         }
      }

   }
}