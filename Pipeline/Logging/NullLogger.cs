using System;

namespace Pipeline.Logging {
    public class NullLogger : BaseLogger, IPipelineLogger {
        public NullLogger() : base(LogLevel.None) {}
        public void Debug(PipelineContext context, string message, params object[] args) {}
        public void Info(PipelineContext context, string message, params object[] args) {}
        public void Warn(PipelineContext context, string message, params object[] args) {}
        public void Error(PipelineContext context, string message, params object[] args) {}
        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {}
    }
}