using Pipeline.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Pipeline.Provider.SqlServer {

    public class SqlMasterUpdater : IUpdate {
        Entity _master;
        EntityOutput _output;

        public SqlMasterUpdater(EntityOutput output) {
            _output = output;
            _master = _output.Context.Process.Entities.First(e => e.IsMaster);
        }

        public void Update() {
            if (_output.Context.Entity.ShouldUpdateMaster()) {
                using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                    cn.Open();
                    var sql = _output.Context.SqlUpdateMaster();
                    var rowCount = cn.Execute(sql, new { TflBatchId = _output.Context.Entity.BatchId, MasterTflBatchId = _master.BatchId }, null, _output.Connection.Timeout, System.Data.CommandType.Text);
                    _output.Context.Info(rowCount + " Updated in {0}", _master.Alias);
                }
            }
        }
    }
}
