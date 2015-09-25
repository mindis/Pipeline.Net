using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {
    /// <summary>
    /// A reader for an entity's input (source).
    /// </summary>
    public class SqlInputReader : IReadInput {

        int _rowCount;
        readonly InputContext _input;
        readonly SqlRowCreator _rowCreator;
        readonly Field[] _fields;

        public SqlInputReader(InputContext input, Field[] fields) {
            _input = input;
            _fields = fields;
            _rowCreator = new SqlRowCreator(input);
        }

        public IEnumerable<Row> Read() {

            using (var cn = new SqlConnection(_input.Connection.GetConnectionString())) {

                cn.Open();
                var cmd = cn.CreateCommand();

                if (_input.Entity.MaxVersion == null) {
                    cmd.CommandText = _input.SqlSelectInput(_fields);
                } else {
                    if (_input.Entity.MinVersion == null) {
                        cmd.CommandText = _input.SqlSelectInputWithMaxVersion(_fields);
                        cmd.Parameters.AddWithValue("@MaxVersion", _input.Entity.MaxVersion);
                    } else {
                        cmd.CommandText = _input.SqlSelectInputWithMinAndMaxVersion(_fields);
                        cmd.Parameters.AddWithValue("@MinVersion", _input.Entity.MinVersion);
                        cmd.Parameters.AddWithValue("@MaxVersion", _input.Entity.MaxVersion);
                    }
                }

                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = _input.Connection.Timeout;

                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                while (reader.Read()) {
                    _rowCount++;
                    _input.Increment();
                    yield return _rowCreator.Create(reader, _input.RowCapacity, _fields);
                }
                _input.Info("{0} from {1}", _rowCount, _input.Connection.Name);
            }
        }

        public void LoadVersion() {
            using (var cn = new SqlConnection(_input.Connection.GetConnectionString())) {
                cn.Open();
                _input.Entity.MaxVersion = _input.Entity.Version == string.Empty ? null : cn.ExecuteScalar(_input.SqlGetInputMaxVersion());
            }
        }

    }
}