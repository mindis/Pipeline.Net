using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityMatchingKeysReader : IEntityOutputKeysReader {

        readonly IConnectionContext _context;
        readonly Field[] _keys;
        readonly Field[] _fields;
        readonly string _insert;
        readonly string _query;
        readonly SqlRowCreator _rowCreator;
        readonly string _tempTable;
        readonly int _rowCapacity;

        public SqlEntityMatchingKeysReader(IConnectionContext context, Field[] keys) {
            _context = context;
            _keys = keys;
            _rowCapacity = context.GetAllEntityFields().Count();
            _fields = CombineFields(_keys, context.Entity.TflHashCode());
            _tempTable = context.Entity.GetExcelName();
            _insert = string.Format("INSERT #{0} VALUES ({1});", _tempTable, string.Join(",", _keys.Select(k => "@" + k.FieldName())));
            var names = string.Join(",", _keys.Select(f => "k.[" + f.FieldName() + "]"));
            var table = context.Entity.OutputTableName(context.Process.Name);
            var joins = string.Join(" AND ", _keys.Select(f => "o.[" + f.FieldName() + "] = k.[" + f.FieldName() + "]"));
            _query = string.Format("SELECT {0},ISNULL(o.[{1}],0) AS [{1}] FROM #{2} k WITH (NOLOCK) INNER JOIN [{3}] o WITH (NOLOCK) ON ({4})", names, context.Entity.TflHashCode().FieldName(), _tempTable, table, joins);
            _rowCreator = new SqlRowCreator(context);
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

                cn.Execute(SqlCreateKeysTable(), null, trans);

                var keys = input.Select(r => r.ToExpandoObject(_keys));
                cn.Execute(_insert, keys, trans, _context.Connection.Timeout, System.Data.CommandType.Text);

                using(var reader = cn.ExecuteReader(_query, null, trans, _context.Connection.Timeout, System.Data.CommandType.Text)) {
                    while (reader.Read()) {
                        results.Add(_rowCreator.Create(reader, _rowCapacity, _fields));
                    }
                }

                cn.Execute(string.Format("DROP TABLE #{0};", _tempTable), null, trans);
                trans.Commit();
            }
            return results.ToArray();
        }

        string SqlCreateKeysTable() {
            var c = _context;
            var columnsAndDefinitions = string.Join(",", c.Entity.GetAllFields().Where(f => f.PrimaryKey).Select(f => "[" + f.FieldName() + "] " + f.SqlDataType() + " NOT NULL"));
            var sql = string.Format(@"CREATE TABLE #{0}({1})", _tempTable, columnsAndDefinitions);
            c.Debug(sql);
            return sql;
        }
    }
}
