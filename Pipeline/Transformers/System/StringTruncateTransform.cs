using Pipeline.Extensions;
using System;
using System.Linq;

namespace Pipeline.Transformers.System {
    public class StringTruncateTransfom : BaseTransform, ITransform {
        private readonly StringLength[] _strings;

        internal class StringLength : IField {
            public int Index { get; set; }
            public int Length { get; set; }
            public StringLength(int index, int length) {
                Index = index;
                Length = length;
            }
        }

        public StringTruncateTransfom(PipelineContext context) : base(context) {
            _strings = context.Entity.GetAllFields().Where(f => f.Type == "string" && f.Length != "max" && f.Output).Select(f => new StringLength(f.Index, Convert.ToInt32(f.Length))).ToArray();
        }

        public Row Transform(Row row) {
            for (int i = 0; i < _strings.Length; i++) {
                var field = _strings[i];
                row[field] = row[field].ToString().Left(field.Length);
            }
            Increment();
            return row;
        }
    }
}
