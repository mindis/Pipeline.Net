using System;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class MinDateTransform : BaseTransform, ITransform {
        private readonly Field[] _dates;
        private readonly DateTime _minDate;

        public MinDateTransform(PipelineContext context, DateTime minDate)
            : base(context) {
            _dates = context.Entity.GetAllFields().Where(f => f.Output && f.Type.StartsWith("date")).ToArray();
            _minDate = minDate;
        }

        public Row Transform(Row row) {
            foreach (var date in _dates.Where(date => (DateTime)row[date] < _minDate)) {
                row[date] = _minDate;
            }
            Increment();
            return row;
        }
    }
}