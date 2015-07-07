using System;

namespace Pipeline.Logging {
    public class LogEntry {
        public PipelineContext Context { get; private set; }
        public DateTime Time { get; private set; }
        public LogLevel Level { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; set; }

        public LogEntry(LogLevel level, PipelineContext context, string message, params object[] args) {
            Time = DateTime.UtcNow;
            Context = context;
            Level = level;
            Message = string.Format(message, args);
        }
    }
}