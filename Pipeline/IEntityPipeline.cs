using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline {
    public interface IEntityPipeline {
        void Initialize();
        void Register(IEntityReader reader);
        void Register(ITransform transformer);
        void Register(IEntityWriter writer);
        void Register(IMasterUpdater updater);
        IEnumerable<Row> Run();
        void Execute();
    }
}