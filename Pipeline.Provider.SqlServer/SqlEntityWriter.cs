using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Extensions;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityWriter : IWrite {
        EntityOutput _output;

        public SqlEntityWriter(EntityOutput output) {
            _output = output;
        }

        public int Write(IEnumerable<Row> rows) {
            var count = 0;
            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();
                foreach (var batch in rows.Partition(_output.Connection.BatchSize)) {
                    var trans = cn.BeginTransaction();
                    var batchCount = cn.Execute(
                        _output.Context.SqlInsertIntoOutput(_output.Context.Entity.BatchId),
                        batch.Select(r => r.ToExpandoObject(_output.OutputFields)),
                        trans,
                        _output.Connection.Timeout,
                        CommandType.Text
                    );
                    trans.Commit();
                    count += batchCount;
                    _output.Increment(batchCount);
                }
                _output.Context.Info("{0} to {1}", count, _output.Connection.Name);
            }
            return count;
        }

        public void LoadVersion() {
            if (_output.Context.Entity.Version == string.Empty)
                return;

            var field = _output.Context.Entity.GetVersionField();

            if (field == null)
                return;

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                _output.Context.Entity.MinVersion = cn.ExecuteScalar(_output.Context.SqlGetOutputMaxVersion(field));
            }
        }
    }
}