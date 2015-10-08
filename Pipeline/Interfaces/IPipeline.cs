using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline.Interfaces {
    public interface IPipeline {
        void Initialize();
        void Register(IRead reader);
        void Register(ITransform transformer);
        void Register(IEnumerable<ITransform> transforms);
        void Register(IWrite writer);
        void Register(IUpdate updater);
        IEnumerable<Row> Run();
        void Execute();
    }

}