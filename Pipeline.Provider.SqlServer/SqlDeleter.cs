using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Extensions;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {
    public class SqlDeleter : IDelete {
        private readonly OutputContext _output;

        public SqlDeleter(OutputContext output) {
            _output = output;
        }

        public void Delete(IEnumerable<Row> rows) {
            var sql = _output.SqlDeleteOutput(_output.Entity.BatchId);
            var count = 0;
            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();
                foreach (var batch in rows.Partition(_output.Entity.DeleteSize)) {
                    var trans = cn.BeginTransaction();
                    var batchCount = cn.Execute(
                        sql,
                        batch.Select(r => r.ToExpandoObject(_output.Entity.GetPrimaryKey())),
                        trans,
                        _output.Connection.Timeout,
                        CommandType.Text
                        );
                    trans.Commit();
                    count += batchCount;
                    _output.Increment(batchCount);
                }
                _output.Entity.Deletes += count;
                _output.Info("{0} deletes from {1}", _output.Entity.Deletes, _output.Connection.Name);
            }
            
        }
    }
}