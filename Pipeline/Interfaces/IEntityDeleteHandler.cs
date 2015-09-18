using System.Collections.Generic;

namespace Pipeline.Interfaces
{
    public interface IEntityDeleteHandler {
        IEnumerable<Row> DetermineDeletes();
        void Delete();
    }
}