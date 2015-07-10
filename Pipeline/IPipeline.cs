using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline {
    public interface IPipeline {
        void Register(IEntityReader reader);
        void Register(ITransform transformer);
        IEnumerable<Row> Run();
    }
}