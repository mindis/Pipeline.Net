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

        public void Write(IEnumerable<Row> rows) {

            using (var cn = new SqlConnection(Connection.GetConnectionString())) {
                cn.Open();
                var count = 0;
                foreach (var batch in rows.Partition(Connection.BatchSize)) {
                    var trans = cn.BeginTransaction();
                    var batchCount = cn.Execute(
                        Context.SqlInsertIntoOutput(Context.Entity.BatchId),
                        batch.Select(r => r.ToExpandoObject(OutputFields)),
                        trans,
                        Connection.Timeout,
                        CommandType.Text
                    );
                    trans.Commit();
                    count += batchCount;
                    Increment(batchCount);
                }
                Context.Info("{0} to {1}", count, Connection.Name);
            }
        }

        public void LoadVersion() {
            if (Context.Entity.Version == string.Empty)
                return;

            var field = Context.Entity.GetVersionField();

            if (field == null)
                return;

            using (var cn = new SqlConnection(Connection.GetConnectionString())) {
                Context.Entity.MinVersion = cn.ExecuteScalar(Context.SqlGetOutputMaxVersion(field));
            }
        }
    }
}