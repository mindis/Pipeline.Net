namespace Pipeline {
    public class NullEntityController : IEntityController {
        public int BatchId { get; private set; }
        public void Initialize() { }
        public void Start() { }
        public void End() { }
    }
}