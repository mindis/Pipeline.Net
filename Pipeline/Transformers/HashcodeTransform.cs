using System.Linq;
using Pipeline.Configuration;
using System.Collections.Generic;
using System.Text;

namespace Pipeline.Transformers {

    public class HashcodeTransform : BaseTransform, ITransform {

        readonly Field[] _input;
        readonly StringBuilder _builder;
        public HashcodeTransform(PipelineContext context)
              : base(context) {
            _input = MultipleInput();
            _builder = new StringBuilder();
        }

        public Row Transform(Row row) {
            row[Context.Field] = GetHashCodeSlow(_input.Select(f => row[f]));
            Increment();
            return row;
        }

        // Jon Skeet's Answer
        // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        // http://eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx
        public static int GetHashCode(IEnumerable<object> values) {
            unchecked {
                int hash = (int)2166136261;
                foreach (var value in values) {
                    hash = hash * 16777619 ^ value.GetHashCode();
                }
                return hash;
            }
        }

        public static int GetHashCode<T>(T a, T b) {
            unchecked {
                int hash = (int)2166136261;
                hash = hash * 16777619 ^ a.GetHashCode();
                hash = hash * 16777619 ^ b.GetHashCode();
                return hash;
            }
        }

        int GetHashCodeSlow(IEnumerable<object> values) {
            _builder.Clear();
            foreach(var value in values) {
                _builder.Append(value);
            }
            return _builder.ToString().GetHashCode();
        }


    }
}