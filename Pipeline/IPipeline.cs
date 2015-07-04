using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline {
    public interface IPipeline {
        void Input(IEntityReader entityReader);
        void Register(ITransformer transformer);
        IEnumerable<Row> Run();
    }
}