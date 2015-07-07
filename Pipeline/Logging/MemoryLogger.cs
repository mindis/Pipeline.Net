using System;
using System.Collections.Generic;

namespace Pipeline.Logging {
    public class MemoryLogger : BaseLogger, IPipelineLogger {
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();

        public MemoryLogger(LogLevel level)
            : base(level) {
        }

        public void Debug(PipelineContext context, string message, params object[] args) {
            if (DebugEnabled) {
                _logEntries.Add(new LogEntry(LogLevel.Debug, context, message, args));
            }
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            if (InfoEnabled) {
                _logEntries.Add(new LogEntry(LogLevel.Info, context, message, args));
            }
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            if (WarnEnabled) {
                _logEntries.Add(new LogEntry(LogLevel.Warn, context, message, args));
            }
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            if (ErrorEnabled) {
                _logEntries.Add(new LogEntry(LogLevel.Error, context, message, args));
            }
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            if (ErrorEnabled) {
                _logEntries.Add(new LogEntry(LogLevel.Error, context, message, args) { Exception = exception });
            }
        }
    }
}