using Pipeline.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Pipeline.Provider.SqlServer {

    public class SqlMasterUpdater : BaseEntityWriter, IMasterUpdater {
        private Entity _master;

        public SqlMasterUpdater(PipelineContext context) : base(context) {
            _master = context.Process.Entities.First(e => e.IsMaster);
        }

        public void Update() {
            if (Context.Entity.IsMaster || Context.Entity.IsFirstRun())
                return;
            using(var cn = new SqlConnection(Connection.GetConnectionString())) {
                cn.Open();
                var sql = Context.SqlUpdateMaster();
                var rowCount = cn.Execute(sql, new { TflBatchId = Context.Entity.BatchId, MasterTflBatchId = _master.BatchId }, null, Connection.Timeout, System.Data.CommandType.Text);
                Context.Info(rowCount + " Updated in {0}", _master.Alias);
            }
        }
    }
}
