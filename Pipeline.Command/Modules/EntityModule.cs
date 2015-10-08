using Autofac;
using Pipeline.Configuration;

namespace Pipeline.Command.Modules {
    public abstract class EntityModule : Module {
        readonly Root _root;

        protected EntityModule(Root root) {
            _root = root;
        }

        protected override void Load(ContainerBuilder builder) {
            foreach (var process in _root.Processes) {
                foreach (var e in process.Entities) {
                    var entity = e;
                    LoadEntity(builder, process, entity);
                }
            }
        }

        public abstract void LoadEntity(ContainerBuilder builder, Process process, Entity entity);
    }
}