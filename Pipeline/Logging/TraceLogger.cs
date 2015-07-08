using System;

namespace Pipeline.Logging {

    public class DebugLogger : BaseLogger, IPipelineLogger {

        private const string FORMAT = "{0:u} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15} {6}";
        public DebugLogger(LogLevel level = LogLevel.Info)
            : base(level) {
        }

        public void Debug(PipelineContext context, string message, params object[] args) {
            if (DebugEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, context.Process, context.Entity, context.Field, context.Transform, "debug", custom);
            }
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            if (InfoEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, context.Process, context.Entity, context.Field, context.Transform, "info", custom);
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            if (WarnEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, context.Process, context.Entity, context.Field, context.Transform, "warn", custom);
            }
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, context.Process, context.Entity, context.Field, context.Transform, "error", custom);
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, context.Process, context.Entity, context.Field, context.Transform, "error", custom);
                System.Diagnostics.Debug.WriteLine(exception.Message);
                System.Diagnostics.Debug.WriteLine(exception.StackTrace);
            }
        }
    }
}