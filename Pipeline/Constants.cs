using System;
using System.Collections.Generic;
using System.Globalization;
using Pipeline.Configuration;

namespace Pipeline {
    public static class Constants {

        static HashSet<string> _types;
        static Dictionary<string, object> _typeDefaults;
        static Dictionary<string, string> _stringDefaults;
        static Dictionary<string, Type> _typeSystem;
        static Dictionary<string, Func<string, bool>> _canConvert;

        public const string ApplicationName = "Pipeline.Net";
        public const string DefaultSetting = "[default]";

        public const string TypeDomain = @"bool,boolean,byte,byte[],char,date,datetime,decimal,double,float,guid,int,int16,int32,int64,long,object,real,short,single,string,uint16,uint32,uint64";

        public const string ComparisonDomain = "Equal,NotEqual,LessThan,GreaterThan,LessThanEqual,GreaterThanEqual";
        public const string ValidatorDomain = "contains";

        public const string TflHashCode = "TflHashCode";
        public const string TflKey = "TflKey";

        public static HashSet<string> TypeSet() {
            return _types ?? (_types = new HashSet<string>(TypeDomain.Split(new[] { ',' })));
        }

        public static Dictionary<string, object> TypeDefaults() {
            return _typeDefaults ?? (
                _typeDefaults = new Dictionary<string, object> {
                    {"bool",false},
                    {"boolean",false},
                    {"byte",default(byte)},
                    {"byte[]",new byte[0]},
                    {"char",default(char)},
                    {"date",DateTime.MaxValue},
                    {"datetime",DateTime.MaxValue},
                    {"decimal",default(decimal)},
                    {"double",default(double)},
                    {"float",default(float)},
                    {"guid",Guid.Parse("00000000-0000-0000-0000-000000000000")},
                    {"int",default(int)},
                    {"int16",default(short)},
                    {"int32",default(int)},
                    {"int64",default(long)},
                    {"long",default(long)},
                    {"object",null},
                    {"real",default(float)},
                    {"short",default(short)},
                    {"single",default(float)},
                    {"string",string.Empty},
                    {"uint16",default(ushort)},
                    {"uint32",default(uint)},
                    {"uint64",default(ulong)},
                });
        }

        public static Dictionary<string, string> StringDefaults() {
            return _stringDefaults ?? (
                _stringDefaults = new Dictionary<string, string> {
                    {"bool","false"},
                    {"boolean","false"},
                    {"byte", default(byte).ToString()},
                    {"byte[]",Field.BytesToHexString(new byte[0])},
                    {"char",default(char).ToString()},
                    {"date","0001-01-01"},
                    {"datetime","0001-01-01"},
                    {"decimal","0.0"},
                    {"double","0.0"},
                    {"float","0.0"},
                    {"guid","00000000-0000-0000-0000-000000000000"},
                    {"int","0"},
                    {"int16","0"},
                    {"int32","0"},
                    {"int64","0"},
                    {"long","0"},
                    {"object",string.Empty},
                    {"real","0.0"},
                    {"short","0"},
                    {"single","0.0"},
                    {"string",string.Empty},
                    {"uint16","0"},
                    {"uint32","0"},
                    {"uint64","0"},
                });
        }

        public static Dictionary<string, Func<string, bool>> CanConvert() {
            bool boolOut;
            byte byteOut;
            char charOut;
            decimal decOut;
            DateTime dateOut;
            double doubleOut;
            float floatOut;
            float singleOut;
            Guid guidOut;
            int intOut;
            short int16Out;
            long longOut;
            UInt16 uInt16Out;
            UInt32 uInt32Out;
            UInt64 uInt64Out;

            return _canConvert ?? (
                _canConvert = new Dictionary<string, Func<string, bool>> {
                    {"bool",s=> Boolean.TryParse(s, out boolOut)},
                    {"boolean",s=> Boolean.TryParse(s, out boolOut)},
                    {"byte",s=>byte.TryParse(s, out byteOut)},
                    {"byte[]", s => false},
                    {"char",s=>char.TryParse(s, out charOut)},
                    {"date",s=> s.Length > 5 && DateTime.TryParse(s, out dateOut)},
                    {"datetime",s=> s.Length > 5 && DateTime.TryParse(s, out dateOut)},
                    {"decimal",s=>decimal.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol, (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(NumberFormatInfo)), out decOut)},
                    {"double",s=>double.TryParse(s, out doubleOut)},
                    {"float",s=>float.TryParse(s, out floatOut)},
                    {"guid", s=>Guid.TryParse(s, out guidOut)},
                    {"int",s=>int.TryParse(s, out intOut)},
                    {"int16", s=>short.TryParse(s, out int16Out)},
                    {"int32",s=>int.TryParse(s, out intOut)},
                    {"int64",s=>long.TryParse(s, out longOut)},
                    {"long",s=>long.TryParse(s, out longOut)},
                    {"object", s=>true},
                    {"real",s=>float.TryParse(s, out singleOut)},
                    {"short",s=>short.TryParse(s, out int16Out)},
                    {"single",s=>float.TryParse(s, out singleOut)},
                    {"string",s=>true},
                    {"uint16",s=>ushort.TryParse(s, out uInt16Out)},
                    {"uint32",s=>uint.TryParse(s, out uInt32Out)},
                    {"uint64",s=>ulong.TryParse(s, out uInt64Out)}
                });
        }

        public static Dictionary<string, Type> TypeSystem() {
            return _typeSystem ?? (
                _typeSystem = new Dictionary<string, Type> {
                    {"bool", typeof(bool)},
                    {"boolean",typeof(bool)},
                    {"byte",typeof(byte)},
                    {"byte[]",typeof(byte[])},
                    {"char",typeof(char)},
                    {"date",typeof(DateTime)},
                    {"datetime",typeof(DateTime)},
                    {"decimal",typeof(decimal)},
                    {"double",typeof(double)},
                    {"float",typeof(float)},
                    {"guid", typeof(Guid)},
                    {"int",typeof(int)},
                    {"int16",typeof(short)},
                    {"int32",typeof(int)},
                    {"int64",typeof(long)},
                    {"long",typeof(long)},
                    {"object",null},
                    {"real",typeof(float)},
                    {"short",typeof(short)},
                    {"single",typeof(float)},
                    {"string",typeof(string)},
                    {"uint16",typeof(ushort)},
                    {"uint32",typeof(uint)},
                    {"uint64",typeof(ulong)},
                });
        }

        public static string GetExcelName(int index) {
            var name = Convert.ToString((char)('A' + (index % 26)));
            while (index >= 26) {
                index = (index / 26) - 1;
                name = Convert.ToString((char)('A' + (index % 26))) + name;
            }
            return name;
        }

    }
}
