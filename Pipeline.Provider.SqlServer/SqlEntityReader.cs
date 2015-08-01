using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityReader : IRead {

        int _rowCount;
        HashSet<int> _errors = new HashSet<int>();
        InputContext _input;

        public SqlEntityReader(InputContext input) {
            _input = input;
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
                    var row = new Row(_input.RowCapacity, _input.Entity.IsMaster);
                    for (var i = 0; i < reader.FieldCount; i++) {
                        var field = _input.InputFields[i];
                        if (field.Type == "string") {
                            if (reader.GetFieldType(i) == typeof(string)) {
                                row.SetString(field, reader.IsDBNull(i) ? null : reader.GetString(i));
                            } else {
                                TypeMismatch(field, reader, i);
                                var value = reader[i];
                                row[field] = value == DBNull.Value ? null : value;
                            }
                        } else {
                            var value = reader[i];
                            row[field] = value == DBNull.Value ? null : value;
                        }
                    }

                    _input.Increment();
                    yield return row;
                }

                _input.Info("{0} from {1}", _rowCount, _input.Connection.Name);
            }
        }

        public void TypeMismatch(Field field, SqlDataReader reader, int index) {
            var key = HashcodeTransform.GetHashCode(new[] { field.Name, field.Type });
            if (_errors.Add(key)) {
                _input.Error("Type mismatch for {0}. Expected {1}, but read {2}.", field.Name, field.Type, reader.GetFieldType(index));
            }
        }

        void LoadVersion(IDbConnection cn) {
            _input.Entity.MaxVersion = _input.Entity.Version == string.Empty ? null : cn.ExecuteScalar(_input.SqlGetInputMaxVersion());
        }
    }
}