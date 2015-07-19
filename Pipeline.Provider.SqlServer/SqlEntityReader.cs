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

        public IEnumerable<Row> Read(object min) {
            using (var cn = new SqlConnection(Connection.GetConnectionString())) {

                cn.Open();
                var cmd = cn.CreateCommand();

                var version = Context.Entity.GetVersionField();
                var max = version == null ? null : GetVersion(cn, version);
                if (max == null) {
                    cmd.CommandText = Context.SqlSelectInput();
                } else {
                    if (min == null) {
                        cmd.CommandText = Context.SqlSelectInputWithMaxVersion(version);
                        cmd.Parameters.AddWithValue("@Version", max);
                    } else {
                        cmd.CommandText = Context.SqlSelectInputWithMinAndMaxVersion(version);
                        cmd.Parameters.AddWithValue("@MinVersion", min);
                        cmd.Parameters.AddWithValue("@MaxVersion", max);
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

        private object GetVersion(IDbConnection cn, Field version) {
            return cn.ExecuteScalar(Context.SqlGetInputMaxVersion(version));
        }
    }
}