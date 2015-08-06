using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pipeline {
    public static class Constants {

        static HashSet<string> _types;
        static Dictionary<string, object> _typeDefaults;
        static Dictionary<string, Type> _typeSystem;
        static Dictionary<string, Func<string, bool>> _canConvert;

        public const string ApplicationName = "Pipeline.Net";
        public const string DefaultSetting = "[default]";

        public const string TypeDomain = @"bool,boolean,byte,byte[],char,date,datetime,decimal,double,float,guid,int,int16,int32,int64,long,object,real,short,single,string,uint16,uint32,uint64";

        public const string ComparisonDomain = "Equal,NotEqual,LessThan,GreaterThan,LessThanEqual,GreaterThanEqual";
        public const string ValidatorDomain = "contains";

        public const string TflHashCode = "TflHashCode";

        public static HashSet<string> TypeSet() {
            return _types ?? (_types = new HashSet<string>(TypeDomain.Split(new[] { ',' })));
        }

        public static Dictionary<string, object> TypeDefaults() {
            return _typeDefaults ?? (
                _typeDefaults = new Dictionary<string, object> {
                    {"bool",false},
                    {"boolean",false},
                    {"byte",default(byte)},
                    {"byte[]",default(byte[])},
                    {"char",default(char)},
                    {"date",default(DateTime)},
                    {"datetime",default(DateTime)},
                    {"decimal",default(decimal)},
                    {"double",default(double)},
                    {"float",default(float)},
                    {"guid",Guid.Parse("00000000-0000-0000-0000-000000000000")},
                    {"int",default(int)},
                    {"int16",default(Int16)},
                    {"int32",default(Int32)},
                    {"int64",default(Int64)},
                    {"long",default(long)},
                    {"object",null},
                    {"real",default(Single)},
                    {"short",default(short)},
                    {"single",default(Single)},
                    {"string",string.Empty},
                    {"uint16",default(UInt16)},
                    {"uint32",default(UInt32)},
                    {"uint64",default(UInt64)},
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
            Single singleOut;
            Guid guidOut;
            int intOut;
            Int16 int16Out;
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
                    {"decimal",s=>Decimal.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol, (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(NumberFormatInfo)), out decOut)},
                    {"double",s=>double.TryParse(s, out doubleOut)},
                    {"float",s=>float.TryParse(s, out floatOut)},
                    {"guid", s=>Guid.TryParse(s, out guidOut)},
                    {"int",s=>int.TryParse(s, out intOut)},
                    {"int16", s=>Int16.TryParse(s, out int16Out)},
                    {"int32",s=>int.TryParse(s, out intOut)},
                    {"int64",s=>long.TryParse(s, out longOut)},
                    {"long",s=>long.TryParse(s, out longOut)},
                    {"object", s=>true},
                    {"real",s=>Single.TryParse(s, out singleOut)},
                    {"short",s=>short.TryParse(s, out int16Out)},
                    {"single",s=>Single.TryParse(s, out singleOut)},
                    {"string",s=>true},
                    {"uint16",s=>UInt16.TryParse(s, out uInt16Out)},
                    {"uint32",s=>UInt32.TryParse(s, out uInt32Out)},
                    {"uint64",s=>UInt64.TryParse(s, out uInt64Out)}
                });
        }

        public static Dictionary<string, Type> TypeSystem() {
            return _typeSystem ?? (
                _typeSystem = new Dictionary<string, Type> {
                    {"bool", typeof(Boolean)},
                    {"boolean",typeof(Boolean)},
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
                    {"int16",typeof(Int16)},
                    {"int32",typeof(Int32)},
                    {"int64",typeof(Int64)},
                    {"long",typeof(long)},
                    {"object",null},
                    {"real",typeof(Single)},
                    {"short",typeof(short)},
                    {"single",typeof(Single)},
                    {"string",typeof(string)},
                    {"uint16",typeof(UInt16)},
                    {"uint32",typeof(UInt32)},
                    {"uint64",typeof(UInt64)},
                });
        }

        public static string GetExcelName(int index) {
            var name = Convert.ToString((char)('A' + (index % 26)));
            while (index >= 26) {
                index = (index / 26) - 1;
                name = System.Convert.ToString((char)('A' + (index % 26))) + name;
            }
            return name;
        }

    }
}
