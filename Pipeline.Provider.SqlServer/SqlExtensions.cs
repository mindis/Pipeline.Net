using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline.Provider.SqlServer {

    public static class SqlExtensions {

        private static Dictionary<string, string> _types;
        private static Dictionary<string, string> Types {
            get {
                return _types ?? (_types = new Dictionary<string, string>
                {
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

        public static string SqlDataType(this Field f) {
            var length = (new[] { "string", "char", "binary", "byte[]", "rowversion", "varbinary" }).Any(t => t == f.Type) ? string.Concat("(", f.Length, ")") : string.Empty;
            var dimensions = (new[] { "decimal" }).Any(s => s.Equals(f.Type)) ? string.Format("({0},{1})", f.Precision, f.Scale) : string.Empty;
            var sqlDataType = Types[f.Type];

            if (!f.Unicode && sqlDataType.StartsWith("N")) {
                sqlDataType = sqlDataType.TrimStart("N".ToCharArray());
            }

            if (!f.VariableLength && sqlDataType.EndsWith("VARCHAR")) {
                sqlDataType = sqlDataType.Replace("VAR", string.Empty);
            }

            return string.Concat(sqlDataType, length, dimensions);
        }

        private static string SqlSchemaPrefix(this PipelineContext c) {
            return c.Entity.Schema == string.Empty ? string.Empty : "[" + c.Entity.Schema + "].";
        }

        private static string SqlOutputName(this PipelineContext c) {
            return c.Entity.OutputName(c.Process.Name);
        }

        public static string SqlInsertIntoOutputStatement(this PipelineContext c, int batchId) {

            var fields = c.Entity.GetAllFields().Where(f => f.Output).ToArray();
            var columns = string.Join(",", fields.Select(f => "[f" + f.Index + "]"));
            var parameters = string.Join(",", fields.Select(f => "@f" + f.Index));

            var sql = string.Format("INSERT {0}{1}({2},TflBatchId) VALUES({3},{4});", SqlSchemaPrefix(c), SqlOutputName(c), columns, parameters, batchId);
            c.Debug(sql);
            return sql;
        }

        public static string SqlDropOutputStatement(this PipelineContext c) {
            var sql = string.Format("DROP TABLE {0}{1};", SqlSchemaPrefix(c), SqlOutputName(c));
            c.Debug(sql);
            return sql;
        }

        public static string SqlCreateOutputStatement(this PipelineContext c) {
            var fields = c.Entity.GetAllFields().Where(f => f.Output).ToArray();
            var columnsAndDefinitions = string.Join(",", fields.Select(f => "[f" + f.Index + "] " + f.SqlDataType() + " NOT NULL"));
            var sql = string.Format("CREATE TABLE {0}({1}, TflBatchId INT NOT NULL, TflKey INT IDENTITY(1,1));", SqlOutputName(c), columnsAndDefinitions);
            c.Debug(sql);
            return sql;
        }

        public static string SqlSelectInputStatement(this PipelineContext c) {
            var fields = c.Entity.GetAllFields().Where(f => f.Input).ToArray();
            var fieldList = string.Join(",", fields.Select(f => "[" + f.Name + "]"));
            var noLock = c.Entity.NoLock ? "WITH (NOLOCK)" : string.Empty;

            var sql = string.Format("SELECT {0} FROM {1}[{2}] {3};", fieldList, SqlSchemaPrefix(c), c.Entity.Name, noLock);
            c.Debug(sql);
            return sql;
        }

        public static string SqlSelectOutputSchemaStatement(this PipelineContext c) {
            var noLock = c.Entity.NoLock ? "WITH (NOLOCK)" : string.Empty;
            var sql = string.Format("SELECT TOP 0 * FROM [{0}] {1};", SqlOutputName(c), noLock);
            c.Debug(sql);
            return sql;
        }

        private static string SqlIdentifier(string name) {
            return Regex.Replace(name, @"[\s\[\]\`]+", "_").Trim("_".ToCharArray()).Left(128);
        }

        public static string SqlCreateOutputUniqueClusteredIndex(this PipelineContext c) {
            var sql = string.Format("CREATE UNIQUE CLUSTERED INDEX [UX_{0}_TflKey] ON [{1}] (TflKey ASC);", SqlIdentifier(SqlOutputName(c)), SqlOutputName(c));
            c.Debug(sql);
            return sql;
        }

        public static string SqlCreateOutputPrimaryKey(this PipelineContext c) {
            var pk = c.Entity.GetAllFields().Where(f => f.PrimaryKey).Select(f => "f" + f.Index).ToArray();
            var keyList = string.Join(", ", pk);
            var sql = string.Format(
                "ALTER TABLE [{0}] ADD CONSTRAINT [PK_{1}_{2}] PRIMARY KEY NONCLUSTERED ({3}) WITH (IGNORE_DUP_KEY = ON);",
                SqlOutputName(c),
                SqlIdentifier(SqlOutputName(c)),
                SqlKeyName(pk),
                keyList
            );
            c.Debug(sql);
            return sql;
        }

        private static string SqlKeyName(string[] pk) {
            return SqlIdentifier(
                string.Join("_", pk)
            );
        }

        public static string GetConnectionString(this Connection c) {
            if (c.ConnectionString != string.Empty) {
                return c.ConnectionString;
            }

            var builder = new SqlConnectionStringBuilder {
                ApplicationName = Constants.ApplicationName,
                ConnectTimeout = c.Timeout,
                DataSource = c.Server,
                InitialCatalog = c.Database,
                IntegratedSecurity = c.User == string.Empty,
                UserID = c.User,
                Password = c.Password
            };

            return builder.ConnectionString;
        }

    }
}