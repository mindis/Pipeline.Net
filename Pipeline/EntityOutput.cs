using System;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {

    public class EntityOutput : IIncrement {
        IIncrement _incrementer;

        public Connection Connection { get; set; }
        public Field[] OutputFields { get; set; }
        public PipelineContext Context { get; private set; }

        public EntityOutput(PipelineContext context, IIncrement incrementer) {
            _incrementer = incrementer;
            Context = context;
            Context.Activity = PipelineActivity.Load;
            OutputFields = context.GetAllEntityOutputFields().ToArray();
            Connection = context.Process.Connections.First(c => c.Name == "output");
        }
        public void Increment(int by = 1) {
            _incrementer.Increment(by);
        }
    }
}