using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityReader : IRead {

        int _rowCount;
        HashSet<int> _errors = new HashSet<int>();
        InputContext _input;
        readonly SqlRowCreator _rowCreator;

        public SqlEntityReader(InputContext input) {
            _input = input;
            _rowCreator = new SqlRowCreator(input);
        }

        public IEnumerable<Row> Read() {

            using (var cn = new SqlConnection(_input.Connection.GetConnectionString())) {

                cn.Open();
                var cmd = cn.CreateCommand();

                LoadVersion(cn);

                if (_input.Entity.MaxVersion == null) {
                    cmd.CommandText = _input.SqlSelectInput();
                } else {
                    if (_input.Entity.MinVersion == null) {
                        cmd.CommandText = _input.SqlSelectInputWithMaxVersion();
                        cmd.Parameters.AddWithValue("@Version", _input.Entity.MaxVersion);
                    } else {
                        if (_input.Entity.MinVersion == _input.Entity.MaxVersion) {
                            yield break;
                        }
                        cmd.CommandText = _input.SqlSelectInputWithMinAndMaxVersion();
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
                    yield return _rowCreator.Create(reader, _input, _input.RowCapacity, _input.InputFields);
                }

                _input.Info("{0} from {1}", _rowCount, _input.Connection.Name);
            }
        }

        void LoadVersion(IDbConnection cn) {
            _input.Entity.MaxVersion = _input.Entity.Version == string.Empty ? null : cn.ExecuteScalar(_input.SqlGetInputMaxVersion());
        }

    }
}