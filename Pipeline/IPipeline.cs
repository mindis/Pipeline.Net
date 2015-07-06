using System.Collections.Generic;
using Pipeline.Transformers;

namespace Pipeline {
    public interface IPipeline {
        void Input(IEnumerable<Row> input);
        void Register(ITransform transformer);
        IEnumerable<Row> Run();
    }
}