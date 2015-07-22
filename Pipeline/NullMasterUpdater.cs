namespace Pipeline {
    public class NullMasterUpdater : IMasterUpdater {
        public PipelineContext Context { get; }

        public NullMasterUpdater(PipelineContext context) {
            Context = context;
        }

        public void Update() { }
    }
}
