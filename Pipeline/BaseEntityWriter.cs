using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class BaseEntityWriter : BaseEntity {
        private readonly IEntityInitializer _initializer;

        public Field[] OutputFields { get; set; }

        public BaseEntityWriter(PipelineContext context, IEntityInitializer initializer)
            : base(context) {
            _initializer = initializer;
            OutputFields = context.Entity.GetAllFields().Where(f => f.Output).ToArray();
            Connection = context.Process.Connections.First(c => c.Name == "output");
        }

        public void Initialize() {
            _initializer.Initialize();
        }

    }
}