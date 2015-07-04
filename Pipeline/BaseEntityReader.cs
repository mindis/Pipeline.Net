using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class BaseEntityReader {
        public BaseEntityReader(Entity entity) {
            Entity = entity;
            RowCapacity = entity.GetAllFields().Count();
            InputFields = entity.Fields.Where(f => f.Input).ToArray();
        }

        public int RowCapacity { get; set; }
        public Entity Entity { get; set; }
        public Field[] InputFields { get; set; }
    }
}
