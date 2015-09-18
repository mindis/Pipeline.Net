using System.Collections.Generic;
using System.Linq;
using Pipeline.Interfaces;

namespace Pipeline.Linq {
    public class ParallelDeleteHandler : IEntityDeleteHandler {
        private readonly IEntityDeleteHandler _deleteHandler;

        public ParallelDeleteHandler(IEntityDeleteHandler deleteHandler) {
            _deleteHandler = deleteHandler;
        }

        public IEnumerable<Row> DetermineDeletes() {
            return _deleteHandler.DetermineDeletes().AsParallel();
        }

        public void Delete() {
            _deleteHandler.Delete();
        }
    }
}