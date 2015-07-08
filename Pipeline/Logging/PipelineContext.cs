using Pipeline.Configuration;

namespace Pipeline.Logging {
    public class PipelineContext {
        public string Process { get; set; }
        public string Entity { get; set; }
        public string Field { get; set; }
        public string Transform { get; set; }

        public PipelineContext(Process process, Entity entity, Field field, Transform transform) {
            Process = process.Name;
            Entity = entity == null ? string.Empty : entity.Alias;
            Field = field == null ? string.Empty : field.Alias;
            Transform = transform == null ? string.Empty : transform.Method;
        }
    }
}