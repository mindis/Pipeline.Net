namespace Pipeline {
    public class Incrementer : IIncrement {
        PipelineContext _context;
        long _rowCount;

        public Incrementer(PipelineContext context) {
            _context = context;
        }
        public void Increment(int by = 1) {
            _rowCount += by;
            if (_rowCount % _context.Entity.LogInterval == 0) {
                _context.Info(_rowCount.ToString());
            }
        }
    }
}
