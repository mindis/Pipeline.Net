namespace Pipeline {
    public class NullEntityController : IEntityController {
        public int BatchId { get; private set; }
        public object StartVersion { get; private set; }
        public object EndVersion { get; private set; }
        public void Start() {}
        public void End() {}
    }
}