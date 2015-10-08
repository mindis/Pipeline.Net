using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Desktop.Actions;
using Pipeline.Interfaces;
using Pipeline.Logging;

namespace Pipeline.Command.Modules {
    public class ActionModule : Module {
        readonly Root _root;

        public ActionModule(Root root) {
            _root = root;
        }

        protected override void Load(ContainerBuilder builder) {
            foreach (var process in _root.Processes) {
                foreach (var action in process.Templates.Where(t => t.Enabled).SelectMany(t => t.Actions).Where(a => a.GetModes().Any(m => m == process.Mode))) {
                    builder.Register(ctx => SwitchAction(ctx, process, action)).Named<IAction>(action.Key);
                }
                foreach (var action in process.Actions.Where(a => a.GetModes().Any(m => m == process.Mode))) {
                    builder.Register(ctx => SwitchAction(ctx, process, action)).Named<IAction>(action.Key);
                }
            }
        }

        private static IAction SwitchAction(IComponentContext ctx, Process process, Action action) {
            var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process);
            switch (action.Name) {
                case "copy":
                    return action.InTemplate ? (IAction)
                        new ContentToFileAction(context, action) :
                        new FileToFileAction(context, action);
                case "web":
                    return new WebAction(context, action);
                default:
                    context.Error("{0} action is not registered.", action.Name);
                    return new NullAction();
            }
        }

    }
}