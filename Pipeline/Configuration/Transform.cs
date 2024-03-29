using Pipeline.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;

namespace Pipeline.Configuration {

    public class Transform : CfgNode, IScript {

        public const string ProducerDomain = "fromxml,fromsplit";
        public const string TransformerDomain = "concat,copy,format,hashcode,htmldecode,left,right,xmldecode,padleft,padright,splitlength,trim,trimstart,trimend,javascript,tostring,toupper,tolower,join,map,decompress,timezone";
        public const string ValidatorDomain = "contains,is";
        public const string TimeZoneIdDomain = "Dateline Standard Time,UTC-11,Samoa Standard Time,Hawaiian Standard Time,Alaskan Standard Time,Pacific Standard Time (Mexico),Pacific Standard Time,US Mountain Standard Time,Mountain Standard Time (Mexico),Mountain Standard Time,Central America Standard Time,Central Standard Time,Central Standard Time (Mexico),Canada Central Standard Time,SA Pacific Standard Time,Eastern Standard Time,US Eastern Standard Time,Venezuela Standard Time,Paraguay Standard Time,Atlantic Standard Time,Central Brazilian Standard Time,SA Western Standard Time,Pacific SA Standard Time,Newfoundland Standard Time,E. South America Standard Time,Argentina Standard Time,SA Eastern Standard Time,Greenland Standard Time,Montevideo Standard Time,UTC-02,Mid-Atlantic Standard Time,Azores Standard Time,Cape Verde Standard Time,Morocco Standard Time,UTC,GMT Standard Time,Greenwich Standard Time,W. Europe Standard Time,Central Europe Standard Time,Romance Standard Time,Central European Standard Time,W. Central Africa Standard Time,Namibia Standard Time,Jordan Standard Time,GTB Standard Time,Middle East Standard Time,Egypt Standard Time,Syria Standard Time,South Africa Standard Time,FLE Standard Time,Israel Standard Time,E. Europe Standard Time,Arabic Standard Time,Arab Standard Time,Russian Standard Time,E. Africa Standard Time,Iran Standard Time,Arabian Standard Time,Azerbaijan Standard Time,Mauritius Standard Time,Georgian Standard Time,Caucasus Standard Time,Afghanistan Standard Time,Ekaterinburg Standard Time,Pakistan Standard Time,West Asia Standard Time,India Standard Time,Sri Lanka Standard Time,Nepal Standard Time,Central Asia Standard Time,Bangladesh Standard Time,N. Central Asia Standard Time,Myanmar Standard Time,SE Asia Standard Time,North Asia Standard Time,China Standard Time,North Asia East Standard Time,Singapore Standard Time,W. Australia Standard Time,Taipei Standard Time,Ulaanbaatar Standard Time,Tokyo Standard Time,Korea Standard Time,Yakutsk Standard Time,Cen. Australia Standard Time,AUS Central Standard Time,E. Australia Standard Time,AUS Eastern Standard Time,West Pacific Standard Time,Tasmania Standard Time,Vladivostok Standard Time,Central Pacific Standard Time,New Zealand Standard Time,UTC+12,Fiji Standard Time,Kamchatka Standard Time,Tonga Standard Time";

        static HashSet<string> _transformSet;
        static HashSet<string> _validateSet;
        static HashSet<string> _producerSet;

        public IScriptParser Parser { get; set; }

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

        [Cfg(value = Constants.DefaultSetting, domain = Constants.DefaultSetting + "," + TimeZoneIdDomain)]
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
        [Cfg(value = "", toLower = true)]
        public string Map { get; set; }

        [Cfg(required = true, toLower = true, domain = TransformerDomain + "," + ValidatorDomain + "," + ProducerDomain)]
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
        [Cfg(value = "Equal", domain = Constants.ComparisonDomain)]
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
        [Cfg(value = "Equal", domain = Constants.ComparisonDomain)]
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

        [Cfg(value = Constants.DefaultSetting, domain = Constants.DefaultSetting + "," + TimeZoneIdDomain)]
        public string ToTimeZone { get; set; }
        [Cfg(value = " ")]
        public string TrimChars { get; set; }
        [Cfg(value = Constants.DefaultSetting, domain = Constants.DefaultSetting + "," + Constants.TypeDomain)]
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

        [Cfg]
        public List<Parameter> Parameters { get; set; }
        [Cfg]
        public List<NameReference> Scripts { get; set; }
        [Cfg]
        public List<NameReference> Templates { get; set; }

        [Cfg]
        public List<Field> Fields { get; set; }

        public bool IsShortHand { get; set; }

        public bool IsValidator() {
            return Validators().Contains(Method);
        }

        [Cfg(value = "firstday", domain = "firstday,firstfourdayweek,firstfullweek", toLower = true)]
        public string CalendarWeekRule { get; set; }

        [Cfg(value = "sunday", domain = "friday,monday,saturday,sunday,tuesday,thursday,wednesday", toLower = true)]
        public string DayOfWeek { get; set; }

        [Cfg]
        public DateTime Date { get; set; }

        protected override void PreValidate() {
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
                    if (String.IsNullOrEmpty(T)) {
                        Error("The shorthand transform requires t attribute.");
                    }
                    break;
                case "format":
                    if (Format == String.Empty) {
                        Error("The format transform requires a format parameter.");
                    } else {
                        if (Format.IndexOf('{') == -1) {
                            Error("The format transform's format must contain a curly braced place-holder.");
                        } else if (Format.IndexOf('}') == -1) {
                            Error("The format transform's format must contain a curly braced place-holder.");
                        }
                    }
                    break;
                case "left":
                case "right":
                    if (Index == 0) {
                        Error("The {0} transform requires a length parameter.", Method);
                    }
                    break;
                case "copy":
                    if (Parameter == String.Empty && !Parameters.Any()) {
                        Error("The copy transform requires at least one parameter.");
                    }
                    break;
                case "javascript":
                    //done at higher level
                    break;
                case "fromsplit":
                case "fromxml":
                    if (!Fields.Any()) {
                        Error("The {0} transform requires a collection of output fields.", Method);
                    }
                    if (Method == "fromsplit" && Separator == Constants.DefaultSetting) {
                        Error("The fromsplit method requires a separator.");
                    }
                    break;
                case "padleft":
                    if (TotalWidth == 0) {
                        Error("The padleft transform requires total width.");
                    }
                    if (PaddingChar == default(char)) {
                        Error("The padleft transform requires a padding character.");
                    }
                    break;
                case "padright":
                    if (TotalWidth == 0) {
                        Error("The padright transform requires total width.");
                    }
                    if (PaddingChar == default(char)) {
                        Error("The padright transform requires a padding character.");
                    }
                    break;
                case "map":
                    if (Map == string.Empty) {
                        Error("The map method requires a map method");
                    }
                    break;
                case "splitlength":
                    if (Separator == Constants.DefaultSetting) {
                        Error("The splitlength transform requires a separator.");
                    }
                    break;
                case "contains":
                    if (Value == string.Empty) {
                        Error("The contains validator requires a value.");
                    }
                    break;
                case "is":
                    if (Type == Constants.DefaultSetting) {
                        Error("The is validator requires a type.");
                    }
                    break;
                case "trimstart":
                case "trimend":
                case "trim":
                    if (TrimChars == string.Empty) {
                        Error("The {0} transform requires trim-chars.", Method);
                    }
                    break;
                case "join":
                    if (Separator == Constants.DefaultSetting) {
                        Error("The {0} transform requires a separator.", Method);
                    }
                    break;
                case "timezone":
                    if (FromTimeZone == Constants.DefaultSetting) {
                        Error("The {0} transform requires from-time-zone to be set.", Method);
                    }
                    if (ToTimeZone == Constants.DefaultSetting) {
                        Error("The {0} transform requires to-time-zone to be set.", Method);
                    }
                    break;
                case "htmldecode":
                case "xmldecode":
                case "tostring":
                case "toupper":
                case "tolower":
                case "decompress":
                    break;
                default:
                    Error("The {0} transform method is undefined.", Method);
                    break;
            }
        }

        public static HashSet<string> Transforms() {
            return _transformSet ?? (_transformSet = new HashSet<string>(TransformerDomain.Split(new[] { ',' })));
        }

        public static HashSet<string> Validators() {
            return _validateSet ?? (_validateSet = new HashSet<string>(ValidatorDomain.Split(new[] { ',' })));
        }

        public static HashSet<string> Producers() {
            return _producerSet ?? (_producerSet = new HashSet<string>(ProducerDomain.Split(new[] { ',' })));
        }

        public override string ToString() {
            return Method;
        }
    }
}