using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityOutputKeysReader : IEntityOutputKeysReader {

        readonly OutputContext _context;
        readonly Field[] _keys;
        readonly Field[] _fields;
        readonly string _insert;
        readonly string _query;
        readonly SqlRowCreator _rowCreator;
        readonly string _tempTable;
        readonly int _rowCapacity;

        public SqlEntityOutputKeysReader(OutputContext context) {
            _context = context;
            _keys = context.Entity.GetAllFields().Where(f => f.PrimaryKey).ToArray();
            _rowCapacity = context.GetAllEntityFields().Count();
            var fields = new List<Field>(_keys);
            var hashCode = context.Entity.CalculatedFields.First(f => f.Name == Constants.TflHashCode);
            fields.Add(hashCode);
            _fields = fields.ToArray();
            _tempTable = context.Entity.GetExcelName();
            _insert = string.Format("INSERT #{0} VALUES ({1});", _tempTable, string.Join(",", _keys.Select(k => "@" + k.FieldName())));
            var names = string.Join(",", _keys.Select(f => "k.[" + f.FieldName() + "]"));
            var table = context.Entity.OutputTableName(context.Process.Name);
            var joins = string.Join(" AND ", _keys.Select(f => "o.[" + f.FieldName() + "] = k.[" + f.FieldName() + "]"));
            _query = string.Format("SELECT {0},ISNULL(o.[{1}],0) AS [{1}] FROM #{2} k WITH (NOLOCK) LEFT OUTER JOIN [{3}] o WITH (NOLOCK) ON ({4})", names, hashCode.FieldName(), _tempTable, table, joins);
            _rowCreator = new SqlRowCreator(context);
        }

        public Row[] Read(IEnumerable<Row> input) {
            var results = new List<Row>();
            using (var cn = new SqlConnection(_context.Connection.GetConnectionString())) {
                cn.Open();
                var trans = cn.BeginTransaction();

                cn.Execute(_context.SqlCreateKeysTable(_tempTable), null, trans);

                var keys = input.Select(r => r.ToExpandoObject(_keys));
                cn.Execute(_insert, keys, trans, _context.Connection.Timeout, System.Data.CommandType.Text);

                using(var reader = cn.ExecuteReader(_query, null, trans, _context.Connection.Timeout, System.Data.CommandType.Text)) {
                    while (reader.Read()) {
                        results.Add(_rowCreator.Create(reader, _context, _rowCapacity, _fields));
                    }
                }

                cn.Execute(string.Format("DROP TABLE #{0};", _tempTable), null, trans);
                trans.Commit();
            }
            return results.ToArray();
        }
    }
}
