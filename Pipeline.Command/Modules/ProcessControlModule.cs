using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;
using Pipeline.Transformers;
using Pipeline.Transformers.System;

namespace Pipeline.Command.Modules {
    public class ProcessControlModule : ProcessModule {

        public ProcessControlModule(Root root) : base(root) { }

        protected override void RegisterProcess(ContainerBuilder builder, Process original) {

            builder.Register<IProcessController>(ctx => {

                var pipelines = new List<IPipeline>();
                var deleteHandlers = new List<IEntityDeleteHandler>();

                // entity-level pipelines
                foreach (var entity in original.Entities) {
                    pipelines.Add(ctx.ResolveNamed<IPipeline>(entity.Key));
                    if (entity.Delete) {
                        deleteHandlers.Add(ctx.ResolveNamed<IEntityDeleteHandler>(entity.Key));
                    }
                }

                // process-level pipeline
                pipelines.Add(ctx.ResolveNamed<IPipeline>(original.Key));

                var outputProvider = original.Connections.First(c => c.Name == "output").Provider;
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), original);

                var controller = new ProcessController(pipelines, deleteHandlers);

                if (original.Mode == "init") {
                    switch (outputProvider) {
                        case "sqlserver":
                            var output = new OutputContext(context, new Incrementer(context));
                            controller.PreActions.Add(new SqlInitializer(output));
                            controller.PostActions.Add(new SqlStarViewCreator(output));
                            break;
                    }
                }

                // templates
                foreach (var template in original.Templates.Where(t => t.Enabled)) {
                    controller.PreActions.Add(new RenderTemplateAction(template, ctx.ResolveNamed<ITemplateEngine>(template.Key)));
                    foreach (var action in template.Actions.Where(a => a.GetModes().Any(m => m == original.Mode))) {
                        if (action.Before) {
                            controller.PreActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                        }
                        if (action.After) {
                            controller.PostActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                        }
                    }
                }

                // actions
                foreach (var action in original.Actions.Where(a => a.GetModes().Any(m => m == original.Mode))) {
                    if (action.Before) {
                        controller.PreActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                    if (action.After) {
                        controller.PostActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                }

                return controller;
            }).Named<IProcessController>(original.Key);
        }


    }
}