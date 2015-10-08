using Pipeline.Interfaces;
using System.Collections.Generic;

namespace Pipeline {

    public class NullWriter : IWrite {
        private readonly IContext _context;

        public NullWriter(IContext context) {
            _context = context;
        }

        public void Write(IEnumerable<Row> rows) {
            _context.Info("Null writing...");
        }

    }
}