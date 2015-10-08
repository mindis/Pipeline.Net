using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Reader;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Template.Razor;

namespace Pipeline.Command.Modules {
    public class TemplateModule : Module {
        readonly Root _root;

        public TemplateModule(Root root) {
            _root = root;
        }
        protected override void Load(ContainerBuilder builder) {
            foreach (var process in _root.Processes) {
                // Using Cfg-Net.Reader to read templates, for now.
                if (process.Templates.Any()) {
                    builder.RegisterType<SourceDetector>().As<ISourceDetector>();
                    builder.RegisterType<FileReader>().Named<IReader>("file");
                    builder.RegisterType<WebReader>().Named<IReader>("web");

                    builder.Register<IReader>(ctx => new DefaultReader(
                        ctx.Resolve<ISourceDetector>(),
                        ctx.ResolveNamed<IReader>("file"),
                        ctx.ResolveNamed<IReader>("web")
                        ));
                }

                foreach (var t in process.Templates.Where(t => t.Enabled)) {
                    var template = t;
                    builder.Register<ITemplateEngine>(ctx => {
                        var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process);
                        switch (template.Engine) {
                            case "razor":
                                return new RazorTemplateEngine(context, template, ctx.Resolve<IReader>());
                            default:
                                return new NullTemplateEngine();
                        }
                    }).Named<ITemplateEngine>(t.Key);
                }
            }
        }
    }
}