using Pipeline.Extensions;
using Pipeline.Interfaces;
using System;
using System.Linq;

namespace Pipeline.Transformers.System {
    public class StringTruncateTransfom : BaseTransform, ITransform {
        readonly StringLength[] _strings;

        internal class StringLength : IField {
            public short Index { get; set; }
            public short MasterIndex { get; set; }
            public int Length { get; set; }

            public string Type => "string";

            public StringLength(short index, short masterIndex, int length) {
                Index = index;
                MasterIndex = masterIndex;
                Length = length;
            }
        }

        public StringTruncateTransfom(PipelineContext context) : base(context) {
            _strings = context.Entity.GetAllFields().Where(f => f.Type == "string" && f.Length != "max" && f.Output).Select(f => new StringLength(f.Index, f.MasterIndex, Convert.ToInt32(f.Length))).ToArray();
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
