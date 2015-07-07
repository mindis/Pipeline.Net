using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Linq;
using Pipeline.Provider.SqlServer;
using Pipeline.Transformers;

namespace Pipeline.Test {

    public class PipelineModule : Module {

        private readonly Root _root;

        public PipelineModule(Root root) {
            _root = root;
        }

        protected override void Load(ContainerBuilder builder) {

            foreach (var p in _root.Processes) {

                var process = p;

                foreach (var e in process.Entities) {

                    var entity = e;

                    var pipeline = process.Pipeline == "defer" ? entity.Pipeline : process.Pipeline;

                    builder.Register<IPipeline>((c) => {
                        switch (pipeline) {
                            case "linq":
                                return new DefaultPipeline();
                            case "parallel.linq":
                                return new Parallel();
                            case "streams":
                                return new Streams.Serial();
                            case "parallel.streams":
                                return new Streams.Parallel();
                            case "linq.optimizer":
                                return new Linq.Optimizer.Serial();
                            case "parallel.linq.optimizer":
                                return new Linq.Optimizer.Parallel();
                            default:
                                return new DefaultPipeline();
                        }
                    }).Named<IPipeline>(e.Key);

                    builder.Register<IEntityReader>((c) => {
                        var connection = process.Connections.First(cn => cn.Name == entity.Connection);
                        switch (connection.Provider) {
                            case "internal":
                                return new DataSetEntityReader(process, entity);
                            case "sqlserver":
                                return new SqlEntityReader(process, entity);
                        }
                        return null;
                    }).Named<IEntityReader>(e.Key);

                    foreach (var f in e.GetAllFields()) {

                        var field = f;

                        foreach (var t in field.Transforms) {

                            var transform = t;

                            builder.Register<ITransform>((c) => {
                                switch (transform.Method) {
                                    case "format":
                                        return new FormatTransform(process, entity, field, transform);
                                    case "left":
                                        return new LeftTransform(process, entity, field, transform);
                                    case "right":
                                        return new RightTransform(process, entity, field, transform);
                                    case "copy":
                                        return new CopyTransform(process, entity, field, transform);
                                    case "concat":
                                        return new ConcatTransform(process, entity, field, transform);
                                    case "fromxml":
                                        return new FromXmlTransform(process, entity, field, transform);
                                    case "htmldecode":
                                        return new HtmlDecodeTransform(process, entity, field, transform);
                                    case "xmldecode":
                                        return new XmlDecodeTransform(process, entity, field, transform);
                                    case "hashcode":
                                        return new HashcodeTransform(process, entity, field, transform);
                                }
                                return new NullTransformer(process, entity, field, transform);
                            }).Named<ITransform>(t.Key);
                        }
                    }
                }

                builder.Register((ctx) => {
                    var pipelines = new List<IPipeline>();
                    foreach (var entity in process.Entities) {
                        var pipeline = ctx.ResolveNamed<IPipeline>(entity.Key);
                        pipeline.Input(ctx.ResolveNamed<IEntityReader>(entity.Key).Read());
                        foreach (var field in entity.GetAllFields()) {
                            foreach (var transform in field.Transforms) {
                                pipeline.Register(ctx.ResolveNamed<ITransform>(transform.Key));
                            }
                        }
                        pipelines.Add(pipeline);
                    }
                    return pipelines;
                }).Named<IEnumerable<IPipeline>>(process.Key);

            }

        }
    }
}