using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Transformers {
    public class ConcatTransform : BaseTransform, ITransform {
        private readonly Field[] _input;

        public ConcatTransform(PipelineContext context)
            : base(context) {
            _input = MultipleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = string.Concat(_input.Select(f => row[f]));
            Increment();
            return row;
        }

        Transform ITransform.InterpretShorthand(string args, List<string> problems) {
            //return InterpretShorthand(args, problems);
            throw new NotImplementedException();
        }

        //public static Transform InterpretShorthand(string args, List<string> problems) {
        //    return Parameterless("concat", "concatenated", args, problems);
        //}

    }
}