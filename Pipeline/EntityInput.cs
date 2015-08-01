using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class EntityInput : IIncrement {
        IIncrement _incrementer;

        public Connection Connection { get; set; }
        public PipelineContext Context { get; private set; }

        public int RowCapacity { get; set; }
        public Field[] InputFields { get; set; }

        public EntityInput(PipelineContext context, IIncrement incrementer) {
            _incrementer = incrementer;
            Context = context;
            Context.Activity = PipelineActivity.Extract;
            RowCapacity = context.GetAllEntityFields().Count();
            InputFields = context.Entity.Fields.Where(f => f.Input).ToArray();
            Connection = context.Process.Connections.First(c => c.Name == context.Entity.Connection);
        }

        public void Increment(int by = 1) {
            _incrementer.Increment(by);
        }

    }
}
