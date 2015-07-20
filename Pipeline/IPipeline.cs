using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline {
    public interface IPipeline {
        void Initialize();
        void Register(IEntityReader reader);
        void Register(ITransform transformer);
        void Register(IEntityWriter writer);
        IEnumerable<Row> Run();
        void Execute();
    }
}