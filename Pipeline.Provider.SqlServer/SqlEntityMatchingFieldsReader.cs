using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityMatchingFieldsReader {

        readonly InputContext _input;
        readonly Field[] _keys;
        readonly string _insert;
        readonly string _query;
        readonly SqlRowCreator _rowCreator;
        readonly string _create;
        readonly string _drop;

        public SqlEntityMatchingFieldsReader(InputContext input) {

            var tempTable = input.Entity.GetExcelName();
            _keys = input.Entity.GetPrimaryKey();
            _input = input;
            _create = SqlCreateKeysTable(input, tempTable);
            _insert = SqlInsertTemplate(input, tempTable);
            _query = SqlQuery(input, tempTable);
            _drop = SqlDrop(input, tempTable);
            _rowCreator = new SqlRowCreator(input);
        }

        static string SqlDrop(IConnectionContext context, string tempTable) {
            var sql = string.Format("DROP TABLE #{0};", tempTable);
            context.Debug(sql);
            return sql;
        }

        static string SqlCreateKeysTable(IConnectionContext context, string tempTable) {
            var columnsAndDefinitions = string.Join(",", context.Entity.GetPrimaryKey().Select(f => "[" + f.FieldName() + "] " + f.SqlDataType() + " NOT NULL"));
            var sql = string.Format(@"CREATE TABLE #{0}({1})", tempTable, columnsAndDefinitions);
            context.Debug(sql);
            return sql;
        }

        static string SqlQuery(InputContext context, string tempTable) {
            var keys = context.Entity.GetPrimaryKey();
            var names = string.Join(",", context.InputFields.Select(f => "i.[" + f.Name + "]"));
            var table = context.SqlSchemaPrefix() + "[" + context.Entity.Name + "]";
            var joins = string.Join(" AND ", keys.Select(f => "i.[" + f.Name + "] = k.[" + f.FieldName() + "]"));
            var noLock = context.Entity.NoLock ? "WITH (NOLOCK)" : string.Empty;
            var sql = string.Format("SELECT {0} FROM #{1} k WITH (NOLOCK) INNER JOIN {2} i {3} ON ({4})", names, tempTable, table, noLock, joins);
            context.Debug(sql);
            return sql;
        }

        string SqlInsertTemplate(IConnectionContext context, string tempTable) {
            var sql = string.Format("INSERT #{0} VALUES ({1});", tempTable, string.Join(",", _keys.Select(k => "@" + k.FieldName())));
            context.Debug(sql);
            return sql;
        }

        static Field[] CombineFields(Field[] keys, Field hashCode) {
            var fields = new List<Field>(keys);
            fields.Add(hashCode);
            return fields.ToArray();
        }

        public IEnumerable<Row> Read(IEnumerable<Row> input) {

            using (var cn = new SqlConnection(_input.Connection.GetConnectionString())) {
                cn.Open();
                var trans = cn.BeginTransaction();

                cn.Execute(_create, null, trans);

                var keys = input.Select(r => r.ToExpandoObject(_keys));
                cn.Execute(_insert, keys, trans, _input.Connection.Timeout, System.Data.CommandType.Text);

                using(var reader = cn.ExecuteReader(_query, null, trans, _input.Connection.Timeout, System.Data.CommandType.Text)) {
                    while (reader.Read()) {
                        yield return _rowCreator.Create(reader, _input.RowCapacity, _input.InputFields);
                    }
                }

                cn.Execute(_drop, null, trans);
                trans.Commit();
            }
        }

    }
}
