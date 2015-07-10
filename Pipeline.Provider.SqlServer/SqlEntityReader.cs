using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityReader : BaseSqlEntityReader, IEntityReader {

        private readonly string _query;
        private int _rowCount;

        public SqlEntityReader(PipelineContext context)
            : base(context) {

            var fieldList = string.Join(",", InputFields.Select(f => "[" + f.Name + "]"));
            var schema = context.Entity.Schema == string.Empty ? string.Empty : "[" + context.Entity.Schema + "].";
            var noLock = context.Entity.NoLock ? "WITH (NOLOCK)" : string.Empty;

            _query = string.Format("SELECT {0} FROM {1}[{2}] {3};", fieldList, schema, context.Entity.Name, noLock);
            Logger.Debug(Context, _query);
        }

        public IEnumerable<Row> Read() {
            using (var cn = new SqlConnection(GetConnectionString())) {

                cn.Open();
                var cmd = cn.CreateCommand();
                cmd.CommandText = _query;
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

                Logger.Info(Context, "Read {0} rows from {1}", _rowCount, Connection.Name);
            }
        }
    }
}