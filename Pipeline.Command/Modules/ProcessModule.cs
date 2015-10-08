using Autofac;
using Pipeline.Configuration;

namespace Pipeline.Command.Modules {
    public abstract class ProcessModule : Module {
        private readonly Root _root;

        protected ProcessModule(Root root) {
            _root = root;
        }

        protected abstract void RegisterProcess(ContainerBuilder builder, Process original);

        protected override void Load(ContainerBuilder builder) {
            foreach (var process in _root.Processes) {
                RegisterProcess(builder, process);
            }
        }
    }
}