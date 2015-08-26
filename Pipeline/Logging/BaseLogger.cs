namespace Pipeline.Logging {
    public class BaseLogger {
        readonly bool _infoEnabled;
        readonly bool _debugEnabled;
        readonly bool _warnEnabled;
        readonly bool _errorEnabled;

        public BaseLogger(LogLevel level) {
            var levels = GetLevels(level);
            _debugEnabled = levels[0];
            _infoEnabled = levels[1];
            _warnEnabled = levels[2];
            _errorEnabled = levels[3];
        }

        public bool InfoEnabled {
            get { return _infoEnabled; }
        }

        public bool DebugEnabled {
            get { return _debugEnabled; }
        }

        public bool WarnEnabled {
            get { return _warnEnabled; }
        }

        public bool ErrorEnabled {
            get { return _errorEnabled; }
        }

        static bool[] GetLevels(LogLevel level) {
            switch (level) {
                case LogLevel.Debug:
                    return new[] { true, true, true, true };
                case LogLevel.Info:
                    return new[] { false, true, true, true };
                case LogLevel.Warn:
                    return new[] { false, false, true, true };
                case LogLevel.Error:
                    return new[] { false, false, false, true };
                case LogLevel.None:
                    return new[] { false, false, false, false };
                default:
                    goto case LogLevel.Info;
            }
        }
    }
}