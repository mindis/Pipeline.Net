using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Extensions;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityUpdater : IWrite {
        OutputContext _output;

        public SqlEntityUpdater(OutputContext output) {
            _output = output;
        }

        public void Write(IEnumerable<Row> rows) {
            var query = _output.SqlUpdateOutput(_output.Entity.BatchId);
            var count = 0;
            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();
                foreach (var batch in rows.Partition(_output.Connection.BatchSize)) {
                    var trans = cn.BeginTransaction();
                    var batchCount = cn.Execute(
                        query,
                        batch.Select(r => r.ToExpandoObject(_output.OutputFields)),
                        trans,
                        _output.Connection.Timeout,
                        CommandType.Text
                    );
                    trans.Commit();
                    count += batchCount;
                    _output.Increment(batchCount);
                }
                _output.Info("{0} to {1}", count, _output.Connection.Name);
            }
            _output.Entity.Updates += count;
        }

        public void LoadVersion() {
            if (_output.Entity.Version == string.Empty)
                return;

            var field = _output.Entity.GetVersionField();

            if (field == null)
                return;

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                _output.Entity.MinVersion = cn.ExecuteScalar(_output.SqlGetOutputMaxVersion(field));
            }
        }
    }
}