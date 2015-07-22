using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityReader : BaseEntityReader, IEntityReader {

        private int _rowCount;

        public SqlEntityReader(PipelineContext context)
            : base(context) {
       } 

        public IEnumerable<Row> Read() {
            using (var cn = new SqlConnection(Connection.GetConnectionString())) {

                cn.Open();
                var cmd = cn.CreateCommand();

                LoadVersion(cn);

                if (Context.Entity.MaxVersion == null) {
                    cmd.CommandText = Context.SqlSelectInput();
                } else {
                    if (Context.Entity.MinVersion == null) {
                        cmd.CommandText = Context.SqlSelectInputWithMaxVersion();
                        cmd.Parameters.AddWithValue("@Version", Context.Entity.MaxVersion);
                    } else {
                        if (Context.Entity.MinVersion == Context.Entity.MaxVersion) {
                            yield break;
                        }
                        cmd.CommandText = Context.SqlSelectInputWithMinAndMaxVersion();
                        cmd.Parameters.AddWithValue("@MinVersion", Context.Entity.MinVersion);
                        cmd.Parameters.AddWithValue("@MaxVersion", Context.Entity.MaxVersion);
                    }
                }

                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Connection.Timeout;

                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                while (reader.Read()) {
                    _rowCount++;
                    var row = new Row(RowCapacity);
                    for (var i = 0; i < reader.FieldCount; i++) {
                        var field = InputFields[i];
                        var value = reader[i];
                        row[field] = value == DBNull.Value ? null : value;
                    }

                    Increment();
                    yield return row;
                }

                Context.Info("{0} from {1}", _rowCount, Connection.Name);
            }
        }

        private void LoadVersion(IDbConnection cn) {
            Context.Entity.MaxVersion = Context.Entity.Version == string.Empty ? null : cn.ExecuteScalar(Context.SqlGetInputMaxVersion());
        }
    }
}