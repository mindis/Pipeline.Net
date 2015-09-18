using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {
    /// <summary>
    /// A reader for an entity's input (source) or output (destination).
    /// </summary>
    public class SqlReader : IRead {
        int _rowCount;
        private readonly IContext _context;
        private readonly Connection _connection;
        private readonly string _tableOrView;
        private readonly Field[] _fields;
        private readonly ReadFrom _readFrom;
        readonly SqlRowCreator _rowCreator;

        public SqlReader(IContext context, Field[] fields, ReadFrom readFrom) {

            _context = context;
            _connection = readFrom == ReadFrom.Output
                ? context.Process.Connections.First(c => c.Name == "output")
                : context.Process.Connections.First(c => c.Name == context.Entity.Connection);
            _tableOrView = readFrom == ReadFrom.Output ? context.Entity.OutputTableName(context.Process.Name) : context.Entity.Name;
            _fields = fields;
            _readFrom = readFrom;

            _rowCreator = new SqlRowCreator(context);
        }

        public IEnumerable<Row> Read() {

            using (var cn = new SqlConnection(_connection.GetConnectionString())) {
                cn.Open();
                var cmd = cn.CreateCommand();

                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $"SELECT [{string.Join("],[", _fields.Select(f => _readFrom == ReadFrom.Output ? f.FieldName() : f.Name))}] FROM [{_tableOrView}] WITH (NOLOCK);";
                _context.Debug(cmd.CommandText);

                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                while (reader.Read()) {
                    _rowCount++;
                    yield return _rowCreator.Create(reader, _fields.Length, _fields);
                }
                _context.Info("{0} from {1}", _rowCount, _connection.Name);
            }
        }
    }
}