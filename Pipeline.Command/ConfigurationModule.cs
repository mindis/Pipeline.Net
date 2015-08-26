using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Reader;
using Pipeline.Configuration;

namespace Pipeline.Command {
    public class ConfigurationModule : Module {
        readonly string _cfg;
        readonly string _shortHand;

        public ConfigurationModule(string cfg, string shortHand) {
            _shortHand = shortHand;
            _cfg = cfg;
        }

        protected override void Load(ContainerBuilder builder) {

            builder.RegisterType<JintParser>().Named<IValidator>("js");
            builder.RegisterType<SourceDetector>().As<ISourceDetector>();
            builder.RegisterType<FileReader>().Named<IReader>("file");
            builder.RegisterType<WebReader>().Named<IReader>("web");

            builder.Register<IReader>((ctx) => new DefaultReader(
                ctx.Resolve<ISourceDetector>(),
                ctx.ResolveNamed<IReader>("file"),
                ctx.ResolveNamed<IReader>("web")
            ));

            builder.Register((ctx) => {
                return new Root(
                    _cfg, 
                    _shortHand, 
                    ctx.ResolveNamed<IValidator>("js"), 
                    ctx.Resolve<IReader>()
                );
            }).As<Root>();

        }
    }
}
