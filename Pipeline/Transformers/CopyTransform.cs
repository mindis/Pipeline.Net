using Pipeline.Configuration;

namespace Pipeline.Transformers {

    public class CopyTransform : BaseTransform, ITransform {

        readonly Field _input;
        public CopyTransform(PipelineContext context)
            : base(context) {
            _input = SingleInput();
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[_input];
            Increment();
            return row;
        }

    }
}