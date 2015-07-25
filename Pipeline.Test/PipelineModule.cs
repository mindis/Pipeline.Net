using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Linq;
using Pipeline.Logging;
using Pipeline.Provider.SqlServer;
using Pipeline.Transformers;
using Pipeline.Validators;
using Pipeline.Transformers.System;

namespace Pipeline.Test {

   public class PipelineModule : Module {

      readonly LogLevel _level;

      public Root Root { get; set; }

      public PipelineModule(string cfg, string shorthand, LogLevel level = LogLevel.Info) {
         _level = level;
         Root = new Root(cfg, shorthand);
      }

      protected override void Load(ContainerBuilder builder) {

         builder.Register<IPipelineLogger>((ctx) => new TraceLogger(_level)).SingleInstance();

         foreach (var p in Root.Processes) {

            var process = p;

            foreach (var e in process.Entities) {

               var entity = e;

               //controller
               builder.Register<IEntityController>((ctx) => {
                  var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                  var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                  switch (provider) {
                     case "sqlserver":
                        return new SqlEntityController(context, new SqlEntityInitializer(context));
                     default:
                        return new NullEntityController();
                  }
               }).Named<IEntityController>(entity.Key);

               var pipeline = process.Pipeline == "defer" ? entity.Pipeline : process.Pipeline;

               builder.Register<IEntityPipeline>((ctx) => {
                  switch (pipeline) {
                     case "parallel.linq":
                        return new Parallel(ctx.ResolveNamed<IEntityController>(entity.Key));
                     case "streams":
                        return new Streams.Serial(ctx.ResolveNamed<IEntityController>(entity.Key));
                     case "parallel.streams":
                        return new Streams.Parallel(ctx.ResolveNamed<IEntityController>(entity.Key));
                     case "linq.optimizer":
                        return new Linq.Optimizer.Serial(ctx.ResolveNamed<IEntityController>(entity.Key));
                     case "parallel.linq.optimizer":
                        return new Linq.Optimizer.Parallel(ctx.ResolveNamed<IEntityController>(entity.Key));
                     default:
                        return new DefaultPipeline(ctx.ResolveNamed<IEntityController>(entity.Key));
                  }
               }).Named<IEntityPipeline>(entity.Key);

               //input
               builder.Register<IEntityReader>((ctx) => {
                  var connection = process.Connections.First(cn => cn.Name == entity.Connection);
                  var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                  switch (connection.Provider) {
                     case "internal":
                        return new DataSetEntityReader(context);
                     case "sqlserver":
                        return new SqlEntityReader(context);
                     default:
                        return new NullEntityReader(context);
                  }
               }).Named<IEntityReader>(entity.Key);

               foreach (var f in entity.GetAllFields().Where(f => f.Transforms.Any())) {

                  var field = f;

                  if (field.RequiresCompositeValidator()) {
                     builder.Register<ITransform>((ctx) => new CompositeValidator(
                         new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field),
                         field.Transforms.Select(t => SwitchTransform(new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, t)))
                     )).Named<ITransform>(field.Key);
                  } else {
                     foreach (var t in field.Transforms) {
                        var transform = t;
                        builder.Register((ctx) => SwitchTransform(new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, transform))).Named<ITransform>(transform.Key);
                     }
                  }
               }

               //output
               builder.Register<IEntityWriter>((ctx) => {
                  var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                  var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                  switch (provider) {
                     case "sqlserver":
                        return new SqlEntityBulkInserter(context);
                     //return new SqlEntityWriter(context);
                     default:
                        return new NullEntityWriter(context);
                  }
               }).Named<IEntityWriter>(entity.Key);

               //master updater
               builder.Register<IMasterUpdater>((ctx) => {
                  var provider = process.Connections.First(cn => cn.Name == "output").Provider;
                  var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity);
                  switch (provider) {
                     case "sqlserver":
                        return new SqlMasterUpdater(context);
                     default:
                        return new NullMasterUpdater(context);
                  }
               }).Named<IMasterUpdater>(entity.Key);

            }

            builder.Register((ctx) => {

               var outputProvider = process.Connections.First(c => c.Name == "output").Provider;
               var pipelines = new List<IEntityPipeline>();

               foreach (var entity in process.Entities) {

                  var pipeline = ctx.ResolveNamed<IEntityPipeline>(entity.Key);
                  pipeline.Initialize();

                  //extract
                  pipeline.Register(ctx.ResolveNamed<IEntityReader>(entity.Key));

                  //transform
                  var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, null, entity.GetDefaultOf<Transform>(t => { t.Method = "defaultnulls"; }));
                  pipeline.Register(new DefaultTransform(context));

                  foreach (var field in context.GetAllEntityFields().Where(f => f.Transforms.Any())) {
                     if (field.RequiresCompositeValidator()) {
                        pipeline.Register(ctx.ResolveNamed<ITransform>(field.Key));
                     } else {
                        foreach (var transform in field.Transforms) {
                           pipeline.Register(ctx.ResolveNamed<ITransform>(transform.Key));
                        }
                     }
                  }

                  //provider specific transforms
                  if (outputProvider == "sqlserver") {
                     var sqlDatesContext = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, null, entity.GetDefaultOf<Transform>(t => { t.Method = "sqldates"; }));
                     pipeline.Register(new MinDateTransform(sqlDatesContext, new DateTime(1753, 1, 1)));
                  }

                  if (outputProvider != "internal") {
                     var stringTruncateContext = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, null, entity.GetDefaultOf<Transform>(t => { t.Method = "truncate"; }));
                     pipeline.Register(new StringTruncateTransfom(stringTruncateContext));
                  }

                  //load
                  pipeline.Register(ctx.ResolveNamed<IEntityWriter>(entity.Key));
                  pipeline.Register(ctx.ResolveNamed<IMasterUpdater>(entity.Key));

                  pipelines.Add(pipeline);
               }
               return pipelines;
            }).Named<IEnumerable<IEntityPipeline>>(process.Key);

         }

      }

      static ITransform SwitchTransform(PipelineContext context) {
         switch (context.Transform.Method) {
            case "format": return new FormatTransform(context);
            case "left": return new LeftTransform(context);
            case "right": return new RightTransform(context);
            case "copy": return new CopyTransform(context);
            case "concat": return new ConcatTransform(context);
            case "fromxml": return new FromXmlTransform(context);
            case "fromsplit": return new FromSplitTransform(context);
            case "htmldecode": return new HtmlDecodeTransform(context);
            case "xmldecode": return new XmlDecodeTransform(context);
            case "hashcode": return new HashcodeTransform(context);
            case "padleft": return new PadLeftTransform(context);
            case "padright": return new PadRightTransform(context);
            case "splitlength": return new SplitLengthTransform(context);
            case "trim": return new TrimTransform(context);
            case "trimstart": return new TrimStartTransform(context);
            case "trimend": return new TrimEndTransform(context);
            case "javascript": return new JintTransform(context);

            case "contains": return new ContainsValidater(context);
            case "is": return new IsValidator(context);

            default: return new NullTransformer(context);
         }
      }
   }
}