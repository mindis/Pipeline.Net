namespace Pipeline {
    public interface IMasterUpdater {
        PipelineContext Context { get; }
        void Update();
    }
}
