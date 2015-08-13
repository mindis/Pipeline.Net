using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityMatchingKeysReader : IEntityMatchingReader {

        readonly IConnectionContext _context;
        readonly Field[] _keys;
        readonly Field[] _fields;
        readonly string _insert;
        readonly string _query;
        readonly SqlRowCreator _rowCreator;
        readonly int _rowCapacity;
        readonly Field _hashCode;
        readonly string _create;
        readonly string _drop;

        public SqlEntityMatchingKeysReader(IConnectionContext context, Field[] keys) {

            var tempTable = context.Entity.GetExcelName();

            _context = context;
            _keys = keys;
            _rowCapacity = context.GetAllEntityFields().Count();
            _hashCode = context.Entity.TflHashCode();
            _fields = CombineFields(keys, _hashCode);
            _create = SqlCreateKeysTable(context, tempTable);
            _insert = SqlInsertTemplate(context, tempTable, keys);
            _query = SqlQuery(keys, context, tempTable, _hashCode);
            _drop = SqlDrop(context, tempTable);
            _rowCreator = new SqlRowCreator(context);
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

        static string SqlQuery(Field[] keys, IConnectionContext context, string tempTable, Field hashCode) {
            var names = string.Join(",", keys.Select(f => "k.[" + f.FieldName() + "]"));
            var table = context.Entity.OutputTableName(context.Process.Name);
            var joins = string.Join(" AND ", keys.Select(f => "o.[" + f.FieldName() + "] = k.[" + f.FieldName() + "]"));
            var sql = string.Format("SELECT {0},o.[{1}] FROM #{2} k WITH (NOLOCK) INNER JOIN [{3}] o WITH (NOLOCK) ON ({4})", names, hashCode.FieldName(), tempTable, table, joins);
            context.Debug(sql);
            return sql;
        }

        static string SqlInsertTemplate(IConnectionContext context, string tempTable, Field[] keys) {
            var sql = string.Format("INSERT #{0} VALUES ({1});", tempTable, string.Join(",", keys.Select(k => "@" + k.FieldName())));
            context.Debug(sql);
            return sql;
        }

        static Field[] CombineFields(Field[] keys, Field hashCode) {
            var fields = new List<Field>(keys);
            fields.Add(hashCode);
            return fields.ToArray();
        }

        public Row[] Read(IEnumerable<Row> input) {
            var results = new List<Row>();
            using (var cn = new SqlConnection(_context.Connection.GetConnectionString())) {
                cn.Open();
                var trans = cn.BeginTransaction();

                cn.Execute(_create, null, trans);

                var keys = input.Select(r => r.ToExpandoObject(_keys));
                cn.Execute(_insert, keys, trans, _context.Connection.Timeout, System.Data.CommandType.Text);

                using(var reader = cn.ExecuteReader(_query, null, trans, _context.Connection.Timeout, System.Data.CommandType.Text)) {
                    while (reader.Read()) {
                        var row = _rowCreator.Create(reader, _rowCapacity, _fields);
                        row.TflHashCode = (int)row[_hashCode];
                        results.Add(row);
                    }
                }

                cn.Execute(_drop, null, trans);
                trans.Commit();
            }
            return results.ToArray();
        }

    }
}
