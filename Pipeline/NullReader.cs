using System.Collections.Generic;
using System.Linq;
using Pipeline.Interfaces;

namespace Pipeline {
    public class NullReader : IRead {
        private readonly IContext _context;

        public NullReader(IContext context) {
            _context = context;
        }

        public IEnumerable<Row> Read() {
            _context.Info("Null Reading...");
            return Enumerable.Empty<Row>();
        }
    }
}