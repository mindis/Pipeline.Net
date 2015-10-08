using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Cfg.Net;
using Pipeline.Configuration;
using Pipeline.Desktop;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;
using Pipeline.Transformers.System;

namespace Pipeline.Command.Modules {
    public class ProcessPipelineModule : ProcessModule {
        public ProcessPipelineModule(Root root) : base(root) { }

        protected override void RegisterProcess(ContainerBuilder builder, Process original) {

            // I need to create a new process with an entity with the appropriate fields

            // clone process, remove entities, and create entity needed for calcuted fields
            var calc = original.Clone() as Process;
            calc.Entities.Clear();
            calc.CalculatedFields.Clear();
            calc.Relationships.Clear();

            var entity = original.GetDefaultOf<Entity>(e => {
                e.Name = "sys.Calc";
                e.Connection = "output";
                e.IsMaster = false;
                e.Fields = new List<Field> {
                    original.GetDefaultOf<Field>(f => {
                        f.Name = Constants.TflKey;
                        f.Type = "int";
                        f.PrimaryKey = true;
                    })
                };
            });

            // Add fields that calculated fields depend on
            entity.Fields.AddRange(original.CalculatedFields
                .SelectMany(f => f.Transforms)
                .SelectMany(t => t.Parameters)
                .Where(p => !p.HasValue() && p.IsField(original))
                .Select(p => p.AsField(original).Clone() as Field)
                );

            entity.CalculatedFields.AddRange(original.CalculatedFields.Select(cf => cf.Clone() as Field));

            calc.Entities.Add(entity);
            calc.ModifyKeys();
            calc.ModifyIndexes();

            // I need a process keyed pipeline
            builder.Register((ctx) => {
                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), calc, entity);
                IPipeline pipeline;
                switch (original.Pipeline) {
                    case "parallel.linq":
                        context.Debug("Registering {0} pipeline.", original.Pipeline);
                        pipeline = new ParallelPipeline(new DefaultPipeline(new NullEntityController(), context));
                        break;
                    default:
                        context.Debug("Registering linq pipeline.", original.Pipeline);
                        pipeline = new DefaultPipeline(new NullEntityController(), context);
                        break;
                }

                // register transforms
                pipeline.Register(new DefaultTransform(context, entity.CalculatedFields));
                pipeline.Register(TransformFactory.GetTransforms(ctx, calc, entity, entity.CalculatedFields));
                pipeline.Register(new StringTruncateTransfom(context));

                // register input and output
                var outputContext = new OutputContext(context, new Incrementer(context));
                switch (outputContext.Connection.Provider) {
                    case "sqlserver":
                        pipeline.Register(new SqlStarParametersReader(outputContext, original));
                        pipeline.Register(new SqlCalculatedFieldUpdater(outputContext, original));
                        pipeline.Register(new MinDateTransform(context, new DateTime(1753, 1, 1)));
                        break;
                    default:
                        pipeline.Register(new NullReader(context));
                        pipeline.Register(new NullWriter(context));
                        break;
                }

                // no updater necessary
                pipeline.Register(new NullUpdater(context, false));

                return pipeline;
            }).Named<IPipeline>(original.Key);

        }
    }
}