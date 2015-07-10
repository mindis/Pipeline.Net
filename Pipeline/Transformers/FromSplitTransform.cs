using System;
using System.Collections.Generic;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class FromSplitTransform : BaseTransform, ITransform {

        private readonly char[] _separator;
        private readonly Field _input;
        private readonly Field[] _output;

        public FromSplitTransform(PipelineContext context)
            : base(context) {
            _input = SingleInputForMultipleOutput();
            _output = MultipleOutput();
            _separator = context.Transform.Separator.ToCharArray();
        }

        public Row Transform(Row row) {
            var values = row[_input].ToString().Split(_separator);
            if (values.Length > 0) {
                for (var i = 0; i < values.Length && i < _output.Length; i++) {
                    var output = _output[i];
                    row[output] = output.Convert(values[i]);
                }
            }
            Increment();
            return row;
        }

        public Transform InterpretShorthand(string args, List<string> problems) {
            throw new NotImplementedException();
        }
    }
}