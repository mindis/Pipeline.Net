using Pipeline.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {

    public class SqlMasterUpdater : IUpdate {
        readonly Entity _master;
        readonly OutputContext _output;

        public SqlMasterUpdater(OutputContext output) {
            _output = output;
            _master = _output.Process.Entities.First(e => e.IsMaster);
        }

        public void Update() {
            if (_output.Entity.ShouldUpdateMaster()) {
                using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                    cn.Open();
                    var sql = _output.SqlUpdateMaster();
                    var rowCount = cn.Execute(sql, new { TflBatchId = _output.Entity.BatchId, MasterTflBatchId = _master.BatchId }, null, _output.Connection.Timeout, System.Data.CommandType.Text);
                    _output.Info(rowCount + " updates to master");
                }
            }
        }
    }
}
