using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityReader : BaseEntityReader, IEntityReader {

        private int _rowCount;

        public SqlEntityReader(PipelineContext context)
            : base(context) {
        }

        public IEnumerable<Row> Read() {
            var selectSql = Context.SqlSelectInputStatement();
            Context.Debug(selectSql);

            var cs = Connection.GetConnectionString();
            Context.Debug(cs);

            using (var cn = new SqlConnection(cs)) {

                cn.Open();
                var cmd = cn.CreateCommand();
                cmd.CommandText = selectSql;
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
    }
}