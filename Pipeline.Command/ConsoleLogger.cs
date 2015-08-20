using Pipeline.Logging;
using System;

namespace Pipeline.Command {

    public class ConsoleLogger : BaseLogger, IPipelineLogger {

        const string FORMAT = "{0:u} | {1} | {2} | {3}";
        const string CONTEXT = "{0} | {1} | {2} | {3} | {4}";
        public ConsoleLogger(LogLevel level = LogLevel.Info)
            : base(level) {
        }

        static string ForLog(PipelineContext context) {
            return string.Format(CONTEXT, context.ForLog);
        }

        public void Debug(PipelineContext context, string message, params object[] args) {
            if (DebugEnabled) {
                var custom = string.Format(message, args);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "debug", custom);
            }
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            if (InfoEnabled) {
                var custom = string.Format(message, args);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "info ", custom);
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            if (WarnEnabled) {
                var custom = string.Format(message, args);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "warn ", custom);
            }
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "error", custom);
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            if (ErrorEnabled) {
                var custom = string.Format(message, args);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(FORMAT, DateTime.UtcNow, ForLog(context), "error", custom);
                Console.Error.WriteLine(exception.Message);
                Console.Error.WriteLine(exception.StackTrace);
            }
        }
    }
}