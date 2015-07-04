using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityReader : BaseSqlEntityReader, IEntityReader {

        private readonly string _query;

        public SqlEntityReader(Entity entity, Connection connection)
            : base(entity, connection) {
            var fieldList = string.Join(",", InputFields.Select(f => "[" + f.Name + "]"));
            _query = string.Format("SELECT {0} FROM [{1}].[{2}];", fieldList, entity.Schema, entity.Name);
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
                    var row = new Row(RowCapacity);
                    for (var i = 0; i < reader.FieldCount; i++) {
                        var field = InputFields[i];
                        row[field] = reader[i];
                    }
                    yield return row;
                }
            }
        }
    }
}