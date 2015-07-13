using Pipeline.Extensions;

namespace Pipeline.Transformers {
    public class RightTransform : BaseTransform, ITransform {

        public RightTransform(PipelineContext context)
            : base(context) {
        }

        public Row Transform(Row row) {
            row[Context.Field] = row[Context.Field].ToString().Right(Context.Transform.Length);
            Increment();
            return row;
        }

    }
}