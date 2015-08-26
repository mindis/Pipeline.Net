using System;

namespace Pipeline.Logging {

    public class DebugLogger : BaseLogger, IPipelineLogger {

        const string FORMAT = "{0:u} | {1} | {2} | {3}";
        const string CONTEXT = "{0} | {1} | {2} | {3} | {4}";
        public DebugLogger(LogLevel level = LogLevel.Info)
            : base(level) {
        }

        static string ForLog(PipelineContext context) {
            return string.Format(CONTEXT, context.ForLog);
        }

        public void Debug(PipelineContext context, string message, params object[] args) {
            if (DebugEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "debug", custom);
            }
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            if (InfoEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "info ", custom);
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            if (WarnEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "warn ", custom);
            }
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "error", custom);
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                System.Diagnostics.Debug.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "error", custom);
                System.Diagnostics.Debug.WriteLine(exception.Message);
                System.Diagnostics.Debug.WriteLine(exception.StackTrace);
            }
        }
    }
}