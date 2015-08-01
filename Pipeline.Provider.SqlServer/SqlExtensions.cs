using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Pipeline.Configuration;
using Pipeline.Extensions;
using System.Text;
using System;

namespace Pipeline.Provider.SqlServer {

   public static class SqlExtensions {

      static Dictionary<string, string> _types;
      static Dictionary<string, string> Types {
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

         if (!f.Unicode && sqlDataType.StartsWith("N", StringComparison.Ordinal)) {
            sqlDataType = sqlDataType.TrimStart("N".ToCharArray());
         }

         if (!f.VariableLength && sqlDataType.EndsWith("VARCHAR", StringComparison.Ordinal)) {
            sqlDataType = sqlDataType.Replace("VAR", string.Empty);
         }

         return string.Concat(sqlDataType, length, dimensions);
      }

      static string SqlSchemaPrefix(this InputContext c) {
         return c.Entity.Schema == string.Empty ? string.Empty : "[" + c.Entity.Schema + "].";
      }

      public static string SqlControlTableName(this OutputContext c) {
         return SqlIdentifier(c.Process.Name) + "Control";
      }

      public static string SqlInsertIntoOutput(this OutputContext c, int batchId) {
         var fields = c.Entity.GetAllFields().Where(f => f.Output).ToArray();
         var parameters = string.Join(",", fields.Select(f => "@f" + f.Index));
         var sql = string.Format("INSERT {0} VALUES({1},{2});", c.Entity.OutputTableName(c.Process.Name), parameters, batchId);
         c.Debug(sql);
         return sql;
      }

      public static string SqlDropOutput(this OutputContext c) {
         var sql = string.Format("DROP TABLE [{0}];", c.Entity.OutputTableName(c.Process.Name));
         c.Debug(sql);
         return sql;
      }

      public static string SqlDropOutputView(this OutputContext c) {
         var sql = string.Format("DROP VIEW [{0}];", c.Entity.OutputViewName(c.Process.Name));
         c.Debug(sql);
         return sql;
      }

      public static string SqlDropControl(this OutputContext c) {
         var sql = string.Format("DROP TABLE [{0}];", SqlControlTableName(c));
         c.Debug(sql);
         return sql;
      }

      static string Enclose(string name) {
         return "[" + name + "]";
      }
      
      public static string SqlUpdateMaster(this OutputContext c) {
         //note: TflBatchId is updated and next process depends it.

         var masterEntity = c.Process.Entities.First(e => e.IsMaster);
         var masterTable = Enclose(masterEntity.OutputTableName(c.Process.Name));
         var masterAlias = masterEntity.GetExcelName();

         var entity = Enclose(c.Entity.OutputTableName(c.Process.Name));
         var entityAlias = c.Entity.GetExcelName();
         var builder = new StringBuilder();

         var sets = c.Entity.Fields.Any(f => f.KeyType.HasFlag(KeyType.Foreign) || f.Denormalize) ?
             string.Join(",", c.Entity.Fields
                 .Where(f => f.KeyType.HasFlag(KeyType.Foreign) || f.Denormalize)
                 .Select(f => string.Concat(masterAlias, ".", f.FieldName(), " = ", entityAlias, ".", f.FieldName())))
             :
             string.Empty;

         builder.AppendFormat("UPDATE {0} SET {1}", masterAlias, sets);
         if (!c.Entity.IsFirstRun()) {
            builder.AppendFormat("{0}{1}.TflBatchId = @TflBatchId", sets == string.Empty ? string.Empty : ",", masterAlias);
         }
         builder.AppendFormat(" FROM {0} {1}", masterTable, masterAlias);

         foreach (var relationship in c.Entity.RelationshipToMaster) {
            var right = Enclose(relationship.Summary.RightEntity.OutputTableName(c.Process.Name));
            var rightEntityAlias = relationship.Summary.RightEntity.GetExcelName();

            builder.AppendFormat(" INNER JOIN {0} {1} ON ( ", right, rightEntityAlias);

            var leftEntity = relationship.Summary.LeftEntity;
            var leftEntityAlias = relationship.Summary.LeftEntity.GetExcelName();

            for (int i = 0; i < relationship.Summary.LeftFields.Count(); i++) {
               var leftAlias = relationship.Summary.LeftFields[i].FieldName();
               var rightAlias = relationship.Summary.RightFields[i].FieldName();
               var conjunction = i > 0 ? " AND " : string.Empty;
               builder.AppendFormat(
                   "{0}{1}.{2} = {3}.{4}",
                   conjunction,
                   leftEntityAlias,
                   Enclose(leftAlias),
                   rightEntityAlias,
                   Enclose(rightAlias)
               );
            }
            builder.AppendLine(")");
         }

         //todo: consider whether or not entity or master changed first
         builder.AppendFormat("WHERE {0}.TflBatchId = @TflBatchId OR {1}.TflBatchId = @MasterTflBatchId", entityAlias, masterAlias);
         var sql = builder.ToString();
         c.Debug(sql);
         return sql;
      }

      public static string SqlControlLastBatchId(this OutputContext c) {
         var sql = string.Format(@"
                SELECT ISNULL(MAX([BatchId]),0) FROM {0};
            ", SqlControlTableName(c));
         c.Debug(sql);
         return sql;
      }

      public static string SqlControlStartBatch(this OutputContext c) {
         var sql = string.Format(@"
                INSERT {0}([BatchId],[Entity],[Inserts],[Updates],[Deletes],[Start],[End]) 
                VALUES(@BatchId,@Entity,0,0,0,GETUTCDATE(),NULL);", SqlControlTableName(c));
         c.Debug(sql);
         return sql;
      }

      public static string SqlControlEndBatch(this OutputContext c) {
         var sql = string.Format(@"
                UPDATE {0}
                SET [Inserts] = @Inserts,
                    [Updates] = @Updates,
                    [Deletes] = @Deletes,
                    [End] = GETUTCDATE()
                WHERE BatchId = @BatchId;", SqlControlTableName(c));
         c.Debug(sql);
         return sql;
      }

      public static string SqlCreateControl(this OutputContext c) {
         var sql = string.Format(@"
                CREATE TABLE {0}(
                    [BatchId] INT NOT NULL,
                    [Entity] NVARCHAR(128) NOT NULL,
                    [Inserts] BIGINT NOT NULL,
                    [Updates] BIGINT NOT NULL,
                    [Deletes] BIGINT NOT NULL,
                    [Start] DATETIME NOT NULL,
                    [End] DATETIME
                    CONSTRAINT PK_{0}_BatchId PRIMARY KEY (
						BatchId ASC,
                        [Entity] ASC
					)
                );
            ", SqlControlTableName(c));
         c.Debug(sql);
         return sql;
      }

      public static string SqlCreateOutput(this OutputContext c) {
         var columnsAndDefinitions = string.Join(",", c.GetAllEntityOutputFields().Select(f => "[" + f.FieldName() + "] " + f.SqlDataType() + " NOT NULL"));
         var sql = string.Format("CREATE TABLE [{0}]({1}, TflBatchId INT NOT NULL, TflKey INT IDENTITY(1,1));", c.Entity.OutputTableName(c.Process.Name), columnsAndDefinitions);
         c.Debug(sql);
         return sql;
      }

      public static string SqlCreateOutputView(this OutputContext c) {
         var columnNames = string.Join(",", c.GetAllEntityOutputFields().Select(f => "[" + f.FieldName() + "] AS [" + f.Alias + "]"));
         var sql = string.Format(@"CREATE VIEW [{0}] AS SELECT {1},TflBatchId,TflKey FROM [{2}] WITH (NOLOCK);", c.Entity.OutputViewName(c.Process.Name), columnNames, c.Entity.OutputTableName(c.Process.Name));
         c.Debug(sql);
         return sql;
      }

      public static string SqlSelectInput(this InputContext c) {
         var fields = c.Entity.GetAllFields().Where(f => f.Input).ToArray();
         var fieldList = string.Join(",", fields.Select(f => "[" + f.Name + "]"));
         var noLock = c.Entity.NoLock ? "WITH (NOLOCK) " : string.Empty;

         var sql = string.Format("SELECT {0} FROM {1}[{2}] {3};", fieldList, SqlSchemaPrefix(c), c.Entity.Name, noLock);
         c.Debug(sql);
         return sql;
      }

      public static string SqlSelectInputWithMaxVersion(this InputContext c) {
         var fieldList = string.Join(",", c.InputFields.Select(f => "[" + f.Name + "]"));
         var noLock = c.Entity.NoLock ? "WITH (NOLOCK) " : string.Empty;

         var sql = string.Format(@"SELECT {0} FROM {1}[{2}] {3} WHERE [{4}] <= @Version;", fieldList, SqlSchemaPrefix(c), c.Entity.Name, noLock, c.Entity.GetVersionField().Name);
         c.Debug(sql);
         return sql;
      }

      public static string SqlSelectInputWithMinAndMaxVersion(this InputContext c) {
         var fields = c.Entity.GetAllFields().Where(f => f.Input).ToArray();
         var fieldList = string.Join(",", fields.Select(f => "[" + f.Name + "]"));
         var noLock = c.Entity.NoLock ? "WITH (NOLOCK) " : string.Empty;
         var sql = string.Format(@"SELECT {0} FROM {1}[{2}] {3} WHERE [{4}] >= @MinVersion AND [{4}] <= @MaxVersion", fieldList, SqlSchemaPrefix(c), c.Entity.Name, noLock, c.Entity.GetVersionField().Name);
         c.Debug(sql);
         return sql;
      }

      public static string SqlGetInputMaxVersion(this InputContext c) {
         var sql = string.Format("SELECT MAX([{0}]) FROM {1}[{2}];", c.Entity.GetVersionField().Name, SqlSchemaPrefix(c), c.Entity.Name);
         c.Debug(sql);
         return sql;
      }

      public static string SqlGetOutputMaxVersion(this OutputContext c, Field version) {
         var sql = string.Format("SELECT MAX([{0}]) FROM [{1}] WITH (NOLOCK);", version.Alias, c.Entity.OutputViewName(c.Process.Name));
         c.Debug(sql);
         return sql;
      }

      public static string SqlSelectOutputSchema(this OutputContext c) {
         var sql = string.Format("SELECT TOP 0 * FROM [{0}] WITH (NOLOCK);", c.Entity.OutputTableName(c.Process.Name));
         c.Debug(sql);
         return sql;
      }

      static string SqlIdentifier(string name) {
         return Regex.Replace(name, @"[\s\[\]\`]+", "_").Trim("_".ToCharArray()).Left(128);
      }

      public static string SqlCreateOutputUniqueClusteredIndex(this OutputContext c) {
         var sql = string.Format("CREATE UNIQUE CLUSTERED INDEX [UX_{0}_TflKey] ON [{1}] (TflKey ASC);", SqlIdentifier(c.Entity.OutputTableName(c.Process.Name)), c.Entity.OutputTableName(c.Process.Name));
         c.Debug(sql);
         return sql;
      }

      public static string SqlCreateOutputPrimaryKey(this OutputContext c) {
         var pk = c.Entity.GetAllFields().Where(f => f.PrimaryKey).Select(f => f.FieldName()).ToArray();
         var keyList = string.Join(", ", pk);
         var sql = string.Format(
             "ALTER TABLE [{0}] ADD CONSTRAINT [PK_{1}_{2}] PRIMARY KEY NONCLUSTERED ({3}) WITH (IGNORE_DUP_KEY = ON);",
             c.Entity.OutputTableName(c.Process.Name),
             SqlIdentifier(c.Entity.OutputTableName(c.Process.Name)),
             SqlKeyName(pk),
             keyList
         );
         c.Debug(sql);
         return sql;
      }

      static string SqlKeyName(string[] pk) {
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