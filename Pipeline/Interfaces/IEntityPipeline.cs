using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline.Interfaces {
    public interface IEntityPipeline {
        void Initialize();
        void Register(IReadInput reader);
        void Register(ITransform transformer);
        void Register(IEnumerable<ITransform> transforms);
        void Register(IWriteOutput writer);
        void Register(IUpdate updater);
        IEnumerable<Row> Run();
        void Execute();
    }

}