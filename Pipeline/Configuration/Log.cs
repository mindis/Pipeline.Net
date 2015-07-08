using Pipeline.Logging;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {
    public class Log : CfgNode {

        [Cfg(value = false)]
        public bool Async { get; set; }

        [Cfg(value = Constants.DefaultSetting, toLower = true)]
        public string Connection { get; set; }

        [Cfg(value = Constants.DefaultSetting)]
        public string File { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string Folder { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string From { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string Layout { get; set; }

        [Cfg(value = "none", domain = "info,error,debug,warn,none", toLower = true)]
        public string Level { get; set; }

        [Cfg(value = "", required = true, unique = true)]
        public string Name { get; set; }

        [Cfg(value = "file", domain = "file,console,mail", ignoreCase = true)]
        public string Provider { get; set; }

        [Cfg(value = Constants.DefaultSetting)]
        public string Subject { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string To { get; set; }
        [Cfg(value = (long)10000)]
        public long Rows { get; set; }

        public LogLevel ToLogLevel() {
            switch (Level) {
                case "info":
                    return LogLevel.Info;
                case "warn":
                    return LogLevel.Warn;
                case "debug":
                    return LogLevel.Debug;
                case "error":
                    return LogLevel.Error;
                default:
                    return LogLevel.None;
            }
        }

    }
}