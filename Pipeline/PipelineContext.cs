using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline {
    public class PipelineContext {
        private PipelineActivity _activity;

        public Process Process { get; set; }
        public Entity Entity { get; set; }
        public Field Field { get; set; }
        public Transform Transform { get; set; }
        public object[] ForLog { get; private set; }

        public PipelineActivity Activity {
            get { return _activity; }
            set {
                _activity = value;
                ForLog[2] = value.ToString().Left(1);
            }
        }

        public PipelineContext(Process process, Entity entity = null, Field field = null, Transform transform = null) {

            Process = process;
            Entity = entity ?? process.GetDefaultOf<Entity>(e => { e.Name = string.Empty; });
            Field = field ?? process.GetDefaultOf<Field>(f => { f.Name = string.Empty; });
            Transform = transform ?? process.GetDefaultOf<Transform>(t => { t.Method = string.Empty; });

            ForLog = new object[5];
            ForLog[0] = process.Name.PadRight(process.LogLimit, ' ').Left(process.LogLimit);
            ForLog[1] = Entity.Alias.PadRight(process.EntityLogLimit, ' ').Left(process.EntityLogLimit);
            ForLog[2] = ' ';
            ForLog[3] = Field.Alias.PadRight(process.FieldLogLimit, ' ').Left(process.FieldLogLimit);
            ForLog[4] = Transform.Method.PadRight(process.TransformLogLimit, ' ').Left(process.TransformLogLimit);
        }
    }
}