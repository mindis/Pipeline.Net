using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Transformers;
using Transformalize.Libs.Cfg.Net;

namespace Pipeline.Configuration {

    public class Transform : CfgNode {

        [Cfg(value = true)]
        public bool AfterAggregation { get; set; }
        [Cfg(value = false)]
        public bool BeforeAggregation { get; set; }
        [Cfg(value = "")]
        public string Characters { get; set; }
        [Cfg(value = "")]
        public string Connection { get; set; }
        [Cfg(value = "All", domain = "All,Any")]
        public string ContainsCharacters { get; set; }
        [Cfg(value = "")]
        public string ContentType { get; set; }
        [Cfg(value = 0)]
        public int Count { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string Data { get; set; }
        [Cfg(value = false)]
        public bool Decode { get; set; }
        [Cfg(value = "")]
        public string Domain { get; set; }
        [Cfg(value = "...")]
        public string Elipse { get; set; }
        [Cfg(value = "")]
        public string Else { get; set; }
        [Cfg(value = false)]
        public bool Encode { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string Encoding { get; set; }
        [Cfg(value = "")]
        public string Format { get; set; }
        [Cfg(value = "0.0")]
        public string FromLat { get; set; }
        [Cfg(value = "0.0")]
        public string FromLong { get; set; }
        [Cfg(value = "")]
        public string FromTimeZone { get; set; }
        [Cfg(value = false)]
        public bool IgnoreEmpty { get; set; }
        [Cfg(value = 0)]
        public int Index { get; set; }
        [Cfg(value = 0)]
        public int Interval { get; set; }
        [Cfg(value = "")]
        public string Left { get; set; }
        [Cfg(value = 0)]
        public int Length { get; set; }
        [Cfg(value = null)]
        public object LowerBound { get; set; }
        [Cfg(value = "Inclusive", domain = "Inclusive,Exclusive,Ignore")]
        public string LowerBoundType { get; set; }
        [Cfg(value = "None")]
        public string LowerUnit { get; set; }
        [Cfg(value = "")]
        public string Map { get; set; }

        [Cfg(required = true, toLower = true)]
        public string Method { get; set; }

        [Cfg(value = "dynamic")]
        public string Model { get; set; }
        [Cfg(value = "")]
        public string Name { get; set; }
        [Cfg(value = false)]
        public bool Negated { get; set; }
        [Cfg(value = "")]
        public string NewValue { get; set; }
        [Cfg(value = "")]
        public string OldValue { get; set; }
        [Cfg(value = "Equal", domain = Constants.ValidComparisons)]
        public string Operator { get; set; }
        [Cfg(value = '0')]
        public char PaddingChar { get; set; }
        [Cfg(value = "")]
        public string Parameter { get; set; }
        [Cfg(value = "")]
        public string Pattern { get; set; }
        [Cfg(value = "")]
        public string Replacement { get; set; }
        [Cfg(value = true)]
        public bool ReplaceSingleQuotes { get; set; }
        [Cfg(value = "")]
        public string Right { get; set; }
        [Cfg(value = "")]
        public string Root { get; set; }
        [Cfg(value = "")]
        public string RunField { get; set; }
        [Cfg(value = "Equal", domain = Constants.ValidComparisons)]
        public string RunOperator { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string RunType { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string RunValue { get; set; }
        [Cfg(value = "")]
        public string Script { get; set; }
        [Cfg(value = Constants.DefaultSetting)]
        public string Separator { get; set; }
        [Cfg(value = 0)]
        public int Sleep { get; set; }
        [Cfg(value = 0)]
        public int StartIndex { get; set; }
        [Cfg(value = "")]
        public string T { get; set; }
        [Cfg(value = "")]
        public string Tag { get; set; }
        [Cfg(value = "")]
        public string TargetField { get; set; }
        [Cfg(value = "")]
        public string Template { get; set; }
        [Cfg(value = "")]
        public string Then { get; set; }
        [Cfg(value = "milliseconds")]
        public string TimeComponent { get; set; }
        [Cfg(value = 0)]
        public int TimeOut { get; set; }
        [Cfg(value = Constants.DefaultSetting, domain = Constants.DefaultSetting + "," + Constants.TypeDomain)]
        public string To { get; set; }
        [Cfg(value = "0.0")]
        public string ToLat { get; set; }
        [Cfg(value = "0.0")]
        public string ToLong { get; set; }
        [Cfg(value = 0)]
        public int TotalWidth { get; set; }
        [Cfg(value = "")]
        public string ToTimeZone { get; set; }
        [Cfg(value = " ")]
        public string TrimChars { get; set; }
        [Cfg(value = "")]
        public string Type { get; set; }
        [Cfg(value = "meters")]
        public string Units { get; set; }
        [Cfg(value = null)]
        public object UpperBound { get; set; }
        [Cfg(value = "Inclusive", domain = "Inclusive,Exclusive,Ignore", ignoreCase = true)]
        public string UpperBoundType { get; set; }
        [Cfg(value = "None")]
        public string UpperUnit { get; set; }
        [Cfg(value = "")]
        public string Url { get; set; }
        [Cfg(value = false)]
        public bool UseHttps { get; set; }
        [Cfg(value = "")]
        public string Value { get; set; }
        [Cfg(value = "GET", domain = "GET,POST", ignoreCase = true)]
        public string WebMethod { get; set; }
        [Cfg(value = "Default")]
        public string XmlMode { get; set; }
        [Cfg(value = "")]
        public string Xpath { get; set; }

        [Cfg()]
        public List<Parameter> Parameters { get; set; }
        [Cfg()]
        public List<NameReference> Scripts { get; set; }
        [Cfg()]
        public List<NameReference> Templates { get; set; }
        [Cfg()]
        public List<Branch> Branches { get; set; }
        [Cfg()]
        public List<Field> Fields { get; set; }

        public bool IsShortHand { get; set; }

        [Cfg(value = "firstday", domain = "firstday,firstfourdayweek,firstfullweek", toLower = true)]
        public string CalendarWeekRule { get; set; }

        [Cfg(value = "sunday", domain = "friday,monday,saturday,sunday,tuesday,thursday,wednesday", toLower = true)]
        public string DayOfWeek { get; set; }

        /// <summary>
        /// Set by Process.ModifyKeys for keyed dependency injection
        /// </summary>
        public string Key { get; set; }

        protected override void Modify() {
            switch (Method) {
                case "trimstartappend":
                    if (Separator.Equals(Constants.DefaultSetting)) {
                        Separator = " ";
                    }
                    break;
            }
        }

        protected override void Validate() {

            switch (Method) {
                case "shorthand":
                    if (string.IsNullOrEmpty(T)) {
                        Error("The shorthand transform requires t attribute.");
                    }
                    break;
                case "format":
                    if (Format == string.Empty) {
                        Error("The format transform requires a format parameter.");
                    }
                    break;
                case "left":
                case "right":
                    if (Index == 0) {
                        Error("The {0} transform requires a length parameter.", Method);
                    }
                    break;
                case "copy":
                    if (Parameter == string.Empty && !Parameters.Any()) {
                        Error("The copy transform requires at least one parameter.");
                    }
                    break;
                case "javascript":
                    ValidateJavascript();
                    break;
                case "fromxml":
                    if (!Fields.Any()) {
                        Error("The fromxml transform requires a collection of output fields.");
                    }
                    break;
                case "htmldecode":
                case "xmldecode":
                    break;
                default:
                    Error("The {0} transform method is undefined.", Method);
                    break;
            }
        }

        private void ValidateJavascript() {
            //TODO: extract interface and inject parser
        }

    }
}