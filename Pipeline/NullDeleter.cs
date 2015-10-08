using System.Collections.Generic;
using Pipeline.Interfaces;

namespace Pipeline {
    public class NullDeleter : IDelete {
        private readonly IContext _context;

        public NullDeleter(IContext context) {
            _context = context;
        }

        public void Delete(IEnumerable<Row> rows) {
            _context.Info("Null Deleting...");
        }
    }
}