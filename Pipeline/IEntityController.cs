namespace Pipeline {
    public interface IEntityController {
        void Initialize();
        int BatchId { get; }
        void Start();
        void End();
    }
}