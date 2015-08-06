using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers.System {
    public class TflHashCodeTransform : BaseTransform, ITransform {
        readonly IOrderedEnumerable<Field> _fieldsToHash;
        readonly Field _tflHashCode;

        public TflHashCodeTransform(PipelineContext context) : base(context) {
            _tflHashCode = context.Entity.TflHashCode();
            _fieldsToHash = context.Entity.Fields.Where(f => f.Input && !f.PrimaryKey).OrderBy(f => f.Index);
        }

        public Row Transform(Row row) {
            row.TflHashCode = HashcodeTransform.GetHashCode(_fieldsToHash.Select(f => row[f]));
            row[_tflHashCode] = row.TflHashCode;
            return row;
        }
    }
}