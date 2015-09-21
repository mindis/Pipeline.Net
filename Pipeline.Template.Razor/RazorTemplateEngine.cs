using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Pipeline.Interfaces;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Pipeline.Template.Razor {

    public class RazorTemplateEngine : ITemplateEngine {

        private readonly PipelineContext _context;
        private readonly Configuration.Template _template;

        // Using Cfg-NET's "Reader" to read content, files, or web addresses with possible parameters.
        private readonly IReader _templateReader;
        private readonly IRazorEngineService _service;

        public RazorTemplateEngine(PipelineContext context, Configuration.Template template, IReader templateReader) {
            _context = context;
            _template = template;
            _templateReader = templateReader;
            var config = new FluentTemplateServiceConfiguration(
                c => c.WithEncoding(_template.ContentType == "html" ? Encoding.Html : Encoding.Raw)
                      .WithCodeLanguage(Language.CSharp)
            );
            _service = RazorEngineService.Create(config);
        }

        public string Render() {

            // get template
            var result = _templateReader.Read(_template.File, new TemplateReaderLogger(_context));
            if (result.Source == Source.Error) {
                _context.Error("Could not read template file {0}", _template.File);
                return string.Empty;
            }

            // get parameters (other than process)
            var parameters = new ExpandoObject();
            foreach (var parameter in _template.Parameters) {
                ((IDictionary<string, object>)parameters).Add(parameter.Name, parameter.Value);
            }
            if (result.Parameters.Any()) {
                foreach (var parameter in result.Parameters) {
                    ((IDictionary<string, object>)parameters)[parameter.Key] = parameter.Value;
                }
            }

            // run it
            return _service.RunCompile(result.Content, _template.Name, null, new {
                _context.Process,
                Parameters = parameters
            });
        }

        internal class TemplateReaderLogger : ILogger {
            private readonly PipelineContext _context;

            public TemplateReaderLogger(PipelineContext context) {
                _context = context;
            }

            public void Warn(string message, params object[] args) {
                _context.Warn(message, args);
            }

            public void Error(string message, params object[] args) {
                _context.Error(message, args);
            }
        }
    }
}
