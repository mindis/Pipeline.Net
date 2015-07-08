using System;
using System.Collections.Generic;

namespace Pipeline {
    public static class Constants {

        private static HashSet<string> _types;
        private static Dictionary<string, object> _typeDefaults;

        public const string ApplicationName = "Pipeline.Net";
        public const string DefaultSetting = "[default]";

        public const string TypeDomain = @"bool,boolean,byte,byte[],char,date,datetime,decimal,double,float,guid,int,int16,int32,int64,long,object,real,short,single,string,uint16,uint32,uint64";

        public const string ComparisonDomain = "Equal,NotEqual,LessThan,GreaterThan,LessThanEqual,GreaterThanEqual";
        public const string ValidatorDomain = "contains";

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
    }
}
