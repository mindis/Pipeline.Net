using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Pipeline.Configuration;
using Pipeline.Extensions;
using System.Text;
using System;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {

    public static class SqlExtensions {

        private static string DefaultValue(Field field) {

            if (field.Default == null)
                return "NULL";

            var d = field.Default == Constants.DefaultSetting ? Constants.StringDefaults()[field.Type] : field.Default;

            if (SqlConstants.StringTypes.Any(t => t == field.Type)) {
                return SqlConstants.T + d + SqlConstants.T;
            }

            if (field.Type.StartsWith("bool")) {
                return d.Equals("true", StringComparison.OrdinalIgnoreCase) ? "1" : "0";
            }

            return d;
        }

        public static string SqlDataType(this Field f) {

            var length = (new[] { "string", "char", "binary", "byte[]", "rowversion", "varbinary" }).Any(t => t == f.Type) ? string.Concat("(", f.Length, ")") : string.Empty;
            var dimensions = (new[] { "decimal" }).Any(s => s.Equals(f.Type)) ?
                $"({f.Precision},{f.Scale})" :
                string.Empty;

            var sqlDataType = SqlConstants.Types[f.Type];

            if (!f.Unicode && sqlDataType.StartsWith("N", StringComparison.Ordinal)) {
                sqlDataType = sqlDataType.TrimStart("N".ToCharArray());
            }

            if (!f.VariableLength && (sqlDataType.EndsWith("VARCHAR", StringComparison.Ordinal) || sqlDataType == "VARBINARY")) {
                sqlDataType = sqlDataType.Replace("VAR", string.Empty);
            }

            return string.Concat(sqlDataType, length, dimensions);
        }

        public static string SqlSchemaPrefix(this IContext c) {
            return c.Entity.Schema == string.Empty ? string.Empty : "[" + c.Entity.Schema + "].";
        }

        public static string SqlControlTableName(this OutputContext c) {
            return SqlIdentifier(c.Process.Name) + "Control";
        }

        public static string SqlInsertIntoOutput(this OutputContext c, int batchId) {
            var fields = c.Entity.GetAllFields().Where(f => f.Output).ToArray();
            var parameters = string.Join(",", fields.Select(f => "@" + f.FieldName()));
            var sql = $"INSERT {c.Entity.OutputTableName(c.Process.Name)} VALUES({parameters},{batchId});";
            c.Debug(sql);
            return sql;
        }

        public static string SqlUpdateOutput(this OutputContext c, int batchId) {
            var fields = c.Entity.GetAllFields().Where(f => f.Output).ToArray();
            var sets = string.Join(",", fields.Where(f => !f.PrimaryKey).Select(f => f.FieldName()).Select(n => "[" + n + "] = @" + n));
            var criteria = string.Join(" AND ", fields.Where(f => f.PrimaryKey).Select(f => f.FieldName()).Select(n => "[" + n + "] = @" + n));
            var sql = $"UPDATE [{c.Entity.OutputTableName(c.Process.Name)}] SET {sets},TflBatchId={batchId} WHERE {criteria};";
            c.Debug(sql);
            return sql;
        }

        public static string SqlUpdateCalculatedFields(this OutputContext c, Process original) {
            var master = original.Entities.First(e => e.IsMaster);
            var fields = c.Entity.CalculatedFields.Where(f => f.Output && f.Name != Constants.TflKey && f.Name != Constants.TflHashCode).ToArray();
            var sets = string.Join(",", fields.Select(f => "[" + original.CalculatedFields.First(cf=>cf.Name == f.Name).FieldName() + "] = @" + f.FieldName()));
            var sql = $"UPDATE [{master.OutputTableName(original.Name)}] SET {sets} WHERE TflKey = @TflKey;";
            c.Debug(sql);
            return sql;
        }

        public static string SqlDeleteOutput(this OutputContext c, int batchId) {
            var criteria = string.Join(" AND ", c.Entity.GetPrimaryKey().Select(f => f.FieldName()).Select(n => "[" + n + "] = @" + n));
            var sql = $"DELETE FROM [{c.Entity.OutputTableName(c.Process.Name)}] WHERE {criteria};";
            c.Debug(sql);
            return sql;
        }

        public static string SqlDropOutput(this OutputContext c) {
            var sql = $"DROP TABLE [{c.Entity.OutputTableName(c.Process.Name)}];";
            c.Debug(sql);
            return sql;
        }

        public static string SqlDropOutputView(this OutputContext c) {
            var sql = $"DROP VIEW [{c.Entity.OutputViewName(c.Process.Name)}];";
            c.Debug(sql);
            return sql;
        }

        public static string SqlDropControl(this OutputContext c) {
            var sql = $"DROP TABLE [{SqlControlTableName(c)}];";
            c.Debug(sql);
            return sql;
        }

        static string Enclose(string name) {
            return SqlConstants.L + name + SqlConstants.R;
        }

        public static string SqlUpdateMaster(this OutputContext c) {
            //note: TflBatchId is updated and next process depends it.

            var masterEntity = c.Process.Entities.First(e => e.IsMaster);
            var masterTable = Enclose(masterEntity.OutputTableName(c.Process.Name));
            var masterAlias = masterEntity.GetExcelName();

            var entityAlias = c.Entity.GetExcelName();
            var builder = new StringBuilder();

            var sets = c.Entity.Fields.Any(f => f.KeyType.HasFlag(KeyType.Foreign) || (c.Entity.Denormalize && f.Output && !f.KeyType.HasFlag(KeyType.Primary))) ?
                string.Join(",", c.Entity.Fields
                    .Where(f => f.KeyType.HasFlag(KeyType.Foreign) || (c.Entity.Denormalize && f.Output && !f.KeyType.HasFlag(KeyType.Primary)))
                    .Select(f => string.Concat(masterAlias, ".", f.FieldName(), " = ", entityAlias, ".", f.FieldName())))
                :
                string.Empty;

            builder.AppendFormat("UPDATE {0} SET {1}", masterAlias, sets);
            if (!c.Entity.IsFirstRun()) {
                builder.AppendFormat("{0}{1}.TflBatchId = @TflBatchId", sets == string.Empty ? string.Empty : ",", masterAlias);
            }
            builder.AppendFormat(" FROM {0} {1}", masterTable, masterAlias);

            foreach (var relationship in c.Entity.RelationshipToMaster.Reverse()) {
                var right = Enclose(relationship.Summary.RightEntity.OutputTableName(c.Process.Name));
                var rightEntityAlias = relationship.Summary.RightEntity.GetExcelName();

                builder.AppendFormat(" INNER JOIN {0} {1} ON ( ", right, rightEntityAlias);

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
            var sql = $"SELECT ISNULL(MAX([BatchId]),0) FROM {SqlControlTableName(c)};";
            c.Debug(sql);
            return sql;
        }

        public static string SqlControlStartBatch(this OutputContext c) {
            var sql = $@"INSERT {SqlControlTableName(c)}([BatchId],[Entity],[Inserts],[Updates],[Deletes],[Start],[End]) VALUES(@BatchId,@Entity,0,0,0,GETUTCDATE(),NULL);";
            c.Debug(sql);
            return sql;
        }

        public static string SqlControlEndBatch(this OutputContext c) {
            var sql = $"UPDATE {SqlControlTableName(c)} SET [Inserts] = @Inserts, [Updates] = @Updates, [Deletes] = @Deletes, [End] = GETDATE() WHERE BatchId = @BatchId;";
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
            var sql = $"CREATE TABLE [{c.Entity.OutputTableName(c.Process.Name)}]({columnsAndDefinitions}, TflBatchId INT NOT NULL, TflKey INT IDENTITY(1,1));";
            c.Debug(sql);
            return sql;
        }

        public static string SqlCreateOutputView(this OutputContext c) {
            var columnNames = string.Join(",", c.GetAllEntityOutputFields().Select(f => "[" + f.FieldName() + "] AS [" + f.Alias + "]"));
            var sql = $@"CREATE VIEW [{c.Entity.OutputViewName(c.Process.Name)}] AS SELECT {columnNames},TflBatchId,TflKey FROM [{c.Entity.OutputTableName(c.Process.Name)}] WITH (NOLOCK);";
            c.Debug(sql);
            return sql;
        }

        public static string SqlDropStarView(this OutputContext c) {
            var sql = $"DROP VIEW [{c.Process.Star}];";
            c.Debug(sql);
            return sql;
        }

        public static string SqlCreateStarView(this IContext c) {
            var starFields = c.Process.GetStarFields().ToArray();
            var master = c.Process.Entities.First(e => e.IsMaster);
            var masterAlias = Constants.GetExcelName(master.Index);
            var masterNames = string.Join(",", starFields[0].Select(f => masterAlias + ".[" + f.FieldName() + "] AS [" + f.Alias + "]"));
            var slaveNames = string.Join(",", starFields[1].Select(f => "ISNULL(" + Constants.GetExcelName(f.EntityIndex) + ".[" + f.FieldName() + "], " + DefaultValue(f) + ") AS [" + f.Alias + "]"));

            var builder = new StringBuilder();

            foreach (var entity in c.Process.Entities.Where(e => !e.IsMaster)) {
                builder.AppendFormat("LEFT OUTER JOIN [{0}] {1} WITH (NOLOCK) ON (", entity.OutputTableName(c.Process.Name), Constants.GetExcelName(entity.Index));

                var relationship = entity.RelationshipToMaster.First();

                foreach (var join in relationship.Join.ToArray()) {
                    var leftField = c.Process.GetEntity(relationship.LeftEntity).GetField(join.LeftField);
                    var rightField = entity.GetField(join.RightField);
                    builder.AppendFormat(
                        "{0}.{1} = {2}.{3} AND ",
                        masterAlias,
                        leftField.FieldName(),
                        Constants.GetExcelName(entity.Index),
                        rightField.FieldName()
                    );
                }

                builder.Remove(builder.Length - 5, 5);
                builder.AppendLine(") ");
            }

            var sql = $"CREATE VIEW[{c.Process.Star}] AS SELECT {masterNames},{slaveNames},{masterAlias}.TflBatchId,{masterAlias}.TflKey FROM[{master.OutputTableName(c.Process.Name)}] {masterAlias} WITH(NOLOCK) {builder};";
            c.Debug(sql);
            return sql;
        }

        public static string SqlSelectInput(this IContext c, Field[] fields) {
            var fieldList = string.Join(",", fields.Select(f => "[" + f.Name + "]"));
            var noLock = c.Entity.NoLock ? "WITH (NOLOCK) " : string.Empty;
            var sql = $"SELECT {fieldList} FROM {SqlSchemaPrefix(c)}[{c.Entity.Name}] {noLock}";
            if (c.Entity.Filter.Any()) {
                sql += " WHERE " + c.ResolveFilter();
            }
            c.Debug(sql);
            return sql;
        }

        public static string SqlSelectInputWithMaxVersion(this IContext c, Field[] fields) {
            var coreSql = SqlSelectInput(c, fields);
            var hasWhere = coreSql.Contains(" WHERE ");
            var sql = $@"{coreSql} {(hasWhere ? " AND " : " WHERE ")} [{c.Entity.GetVersionField().Name}] <= @MaxVersion";
            c.Debug(sql);
            return sql;
        }

        public static string SqlSelectInputWithMinAndMaxVersion(this IContext c, Field[] fields) {
            var coreSql = SqlSelectInputWithMaxVersion(c, fields);
            var sql = $"{coreSql} AND [{c.Entity.GetVersionField().Name}] >= @MinVersion";
            c.Debug(sql);
            return sql;
        }

        public static string SqlGetInputMaxVersion(this IContext c) {
            var sql = $"SELECT MAX([{c.Entity.GetVersionField().Name}]) FROM {SqlSchemaPrefix(c)}[{c.Entity.Name}];";
            c.Debug(sql);
            return sql;
        }

        public static string SqlGetOutputMaxVersion(this OutputContext c, Field version) {
            var sql = $"SELECT MAX([{version.Alias}]) FROM [{c.Entity.OutputViewName(c.Process.Name)}] WITH (NOLOCK);";
            c.Debug(sql);
            return sql;
        }

        public static string SqlSelectOutputSchema(this OutputContext c) {
            var sql = $"SELECT TOP 0 * FROM [{c.Entity.OutputTableName(c.Process.Name)}] WITH (NOLOCK);";
            c.Debug(sql);
            return sql;
        }

        static string SqlIdentifier(string name) {
            return Regex.Replace(name, @"[\s\[\]\`]+", "_").Trim("_".ToCharArray()).Left(128);
        }

        public static string SqlCreateOutputUniqueClusteredIndex(this OutputContext c) {
            var sql = $"CREATE UNIQUE CLUSTERED INDEX [UX_{SqlIdentifier(c.Entity.OutputTableName(c.Process.Name))}_TflKey] ON [{c.Entity.OutputTableName(c.Process.Name)}] (TflKey ASC);";
            c.Debug(sql);
            return sql;
        }

        public static string SqlCreateOutputPrimaryKey(this OutputContext c) {
            var pk = c.Entity.GetAllFields().Where(f => f.PrimaryKey).Select(f => f.FieldName()).ToArray();
            var keyList = string.Join(", ", pk);
            var sql = $"ALTER TABLE [{c.Entity.OutputTableName(c.Process.Name)}] ADD CONSTRAINT [PK_{SqlIdentifier(c.Entity.OutputTableName(c.Process.Name))}_{SqlKeyName(pk)}] PRIMARY KEY NONCLUSTERED ({keyList}) WITH (IGNORE_DUP_KEY = ON);";
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

            return (new SqlConnectionStringBuilder {
                ApplicationName = Constants.ApplicationName,
                ConnectTimeout = c.Timeout,
                DataSource = c.Server,
                InitialCatalog = c.Database,
                IntegratedSecurity = c.User == string.Empty,
                UserID = c.User,
                Password = c.Password
            }).ConnectionString;
        }
    }
}