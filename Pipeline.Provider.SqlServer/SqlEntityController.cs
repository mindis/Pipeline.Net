using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityController : IEntityController {
        private readonly PipelineContext _context;
        private readonly Connection _output;

        public int BatchId { get; private set; }
        public object StartVersion { get; private set; }

        public SqlEntityController(PipelineContext context) {
            _context = context;
            _output = context.Process.Connections.First(c => c.Name == "output");
            BatchId = 0;
            StartVersion = null;
        }

        private int GetBatchId(IDbConnection cn) {
            var batchId = cn.Query<int>(_context.SqlControlLastBatchId()).FirstOrDefault() + 1;
            _context.Info("Batch " + batchId);
            return batchId;
        }

        public void Start() {
            using (var cn = new SqlConnection(_output.GetConnectionString())) {
                cn.Open();
                BatchId = GetBatchId(cn);
                cn.Execute(_context.SqlControlStartBatch(), new { BatchId, Entity = _context.Entity.Alias });
            }
        }

        public void End() {
            using (var cn = new SqlConnection(_output.GetConnectionString())) {
                cn.Open();
                cn.Execute(_context.SqlControlEndBatch(), new { Inserts = 0, Updates = 0, Deletes = 0, BatchId });
            }
        }
    }
}