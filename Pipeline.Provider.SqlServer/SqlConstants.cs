using System.Collections.Generic;

namespace Pipeline.Provider.SqlServer {
    public class SqlConstants {
        public static string[] StringTypes { get; internal set; } = { "string", "char", "datetime", "guid", "xml" };
        public static string T { get; internal set; } = "'";
        public static string L { get; internal set; } = "[";
        public static string R { get; internal set; } = "]";

        static Dictionary<string, string> _types;
        public static Dictionary<string, string> Types => _types ?? (_types = new Dictionary<string, string> {
            {"int64", "BIGINT"},
            {"int", "INT"},
            {"long", "BIGINT"},
            {"boolean", "BIT"},
            {"bool", "BIT"},
            {"string", "NVARCHAR"},
            {"datetime", "DATETIME"},
            {"date", "DATETIME"},
            {"time", "DATETIME"},
            {"decimal", "DECIMAL"},
            {"numeric", "DECIMAL"},
            {"double", "FLOAT"},
            {"int32", "INT"},
            {"char", "NCHAR"},
            {"single", "REAL"},
            {"int16", "SMALLINT"},
            {"byte", "TINYINT"},
            {"byte[]", "VARBINARY"},
            {"guid", "UNIQUEIDENTIFIER"},
            {"rowversion", "BINARY"},
            {"xml", "XML"}
        });

    }
}
