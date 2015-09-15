using System.Collections.Generic;

namespace Pipeline.Interfaces {
    public interface IProcessController {
        void PreExecute();
        void Execute();
        void PostExecute();
        IEnumerable<Row> Run();
    }    
}
