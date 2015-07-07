using System;

namespace Pipeline.Logging {
    public interface IPipelineLogger {
        void Debug(PipelineContext context, string message, params object[] args);
        void Info(PipelineContext context, string message, params object[] args);
        void Warn(PipelineContext context, string message, params object[] args);
        void Error(PipelineContext context, string message, params object[] args);
        void Error(PipelineContext context, Exception exception, string message, params object[] args);
    }
}