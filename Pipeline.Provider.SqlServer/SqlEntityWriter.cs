using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Extensions;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityWriter : BaseEntityWriter, IEntityWriter {

        public SqlEntityWriter(PipelineContext context)
            : base(context) {
        }

        public void Write(IEnumerable<Row> rows, int batchId) {

            using (var cn = new SqlConnection(Connection.GetConnectionString())) {
                cn.Open();
                var count = 0;
                foreach (var batch in rows.Partition(Connection.BatchSize)) {
                    var batchCount = cn.Execute(Context.SqlInsertIntoOutput(batchId), batch.Select(r => r.ToExpandoObject(OutputFields, batchId)), null, Connection.Timeout, CommandType.Text);
                    count += batchCount;
                    Increment(batchCount);
                }
                Context.Info("{0} to {1}", count, Connection.Name);
            }
        }

        public object GetVersion() {
            if (Context.Entity.Version == string.Empty)
                return null;

            var field = Context.Entity.GetVersionField();

            if (field == null)
                return null;

            using (var cn = new SqlConnection(Connection.GetConnectionString())) {
                return cn.Query<object>(Context.SqlGetOutputMaxVersion(field)).SingleOrDefault();
            }
        }
    }
}