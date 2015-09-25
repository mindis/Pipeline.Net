using System;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Desktop.Transformers {

    public class TimeZoneOperation : BaseTransform, ITransform {
        readonly Field _input;
        readonly Field _output;
        private readonly TimeZoneInfo _toTimeZoneInfo;
        private readonly TimeSpan _adjustment;
        private readonly TimeSpan _daylightAdjustment;

        public TimeZoneOperation(PipelineContext context) : base(context) {
            _input = SingleInput();
            _output = context.Field;

            var fromTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(context.Transform.FromTimeZone);
            _toTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(context.Transform.ToTimeZone);

            _adjustment = _toTimeZoneInfo.BaseUtcOffset - fromTimeZoneInfo.BaseUtcOffset;
            _daylightAdjustment = _adjustment.Add(new TimeSpan(0, 1, 0, 0));
        }

        public Row Transform(Row row) {
            Increment();
            var date = (DateTime)row[_input];
            if (_toTimeZoneInfo.IsDaylightSavingTime(DateTime.Now)) {
                row[_output] = date.Add(_daylightAdjustment);
            } else {
                row[_output] = date.Add(_adjustment);
            }
            return row;
        }
    }
}