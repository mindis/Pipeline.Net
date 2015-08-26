using System;
using System.Diagnostics;
using Pipeline.Logging;

namespace Pipeline.Test {

    public class TraceLogger : BaseLogger, IPipelineLogger {

        const string FORMAT = "{0:u} | {1} | {2} | {3}";
        const string CONTEXT = "{0} | {1} | {2} | {3} | {4}";
        public TraceLogger(LogLevel level = LogLevel.Info)
            : base(level) {
        }

        static string ForLog(PipelineContext context) {
            return string.Format(CONTEXT, context.ForLog);
        }

        public void Debug(PipelineContext context, string message, params object[] args) {
            if (DebugEnabled) {
                var custom = string.Format(message, args);
                Trace.WriteLine(string.Format(FORMAT, DateTime.UtcNow, ForLog(context), "debug", custom), "debug");
            }
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            if (InfoEnabled) {
                var custom = string.Format(message, args);
                Trace.WriteLine(string.Format(FORMAT, DateTime.UtcNow, ForLog(context), "info ", custom), "info ");
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            if (WarnEnabled) {
                var custom = string.Format(message, args);
                Trace.WriteLine(string.Format(FORMAT, DateTime.UtcNow, ForLog(context), "warn ", custom), "warn ");
            }
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                Trace.WriteLine(string.Format(FORMAT, DateTime.UtcNow, ForLog(context), "error", custom), "error");
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                Trace.WriteLine(string.Format(FORMAT, DateTime.UtcNow, ForLog(context), "error", custom));
                Trace.WriteLine(exception.Message);
                Trace.WriteLine(exception.StackTrace);
            }
        }
    }
}