using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Pipeline.Provider.SqlServer {

    public class SqlInputReader : IInputReader {

        private readonly string _connectionString;
        private readonly string _query;
        private readonly int _totalFieldCount;

        public SqlInputReader(string connectionString, string query, int totalFieldCount) {
            _connectionString = connectionString;
            _query = query;
            _totalFieldCount = totalFieldCount;
        }

        public IEnumerable<Row> Read() {
            using (var cn = new SqlConnection(_connectionString)) {
                cn.Open();
                var cmd = cn.CreateCommand();
                cmd.CommandText = _query;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    var row = new Row(_totalFieldCount);

                    for (int i = 0; i < reader.FieldCount; i++) {
                        row[i] = reader[i];
                    }
                    yield return row;
                }
            }
        }
    }
}