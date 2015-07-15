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
                try {
                    cn.Execute(Context.SqlDropOutputStatement());
                } catch { } finally {
                    cn.Execute(Context.SqlCreateOutputStatement());
                    cn.Execute(Context.SqlCreateOutputUniqueClusteredIndex());
                    cn.Execute(Context.SqlCreateOutputPrimaryKey());
                }
                const int batchId = 0;
                var count = 0;
                foreach (var batch in rows.Partition(Connection.BatchSize)) {
                    var batchCount = cn.Execute(Context.SqlInsertIntoOutputStatement(batchId), batch.Select(r => r.ToExpandoObject(OutputFields, batchId)), null, Connection.Timeout, CommandType.Text);
                    count += batchCount;
                    Increment(batchCount);
                }
                Context.Info("{0} to {1}", count, Connection.Name);
            }
        }
    }
}