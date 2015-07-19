namespace Pipeline {
    public interface IEntityController {
        int BatchId { get; }
        void Start();
        void End();
    }
}