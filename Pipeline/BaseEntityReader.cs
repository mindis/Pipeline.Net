using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class BaseEntityReader {

        public BaseEntityReader(Process process, Entity entity) {
            Process = process;
            Entity = entity;

            RowCapacity = entity.GetAllFields().Count();
            InputFields = entity.Fields.Where(f => f.Input).ToArray();
        }

        public int RowCapacity { get; set; }
        public Process Process { get; private set; }
        public Entity Entity { get; private set; }
        public Field[] InputFields { get; set; }
    }
}
