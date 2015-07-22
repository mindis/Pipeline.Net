using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityController : IEntityController {
        private readonly PipelineContext _context;
        private readonly IEntityInitializer _initializer;
        private readonly Connection _output;

        public object StartVersion { get; private set; }

        public SqlEntityController(PipelineContext context, IEntityInitializer initializer) {
            _context = context;
            _initializer = initializer;
            _output = context.Process.Connections.First(c => c.Name == "output");
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
                _context.Entity.BatchId = GetBatchId(cn);
                cn.Execute(_context.SqlControlStartBatch(), new { _context.Entity.BatchId, Entity = _context.Entity.Alias });
            }
        }

        public void End() {
            using (var cn = new SqlConnection(_output.GetConnectionString())) {
                cn.Open();
                cn.Execute(_context.SqlControlEndBatch(), new { Inserts = 0, Updates = 0, Deletes = 0, _context.Entity.BatchId });
            }
        }

        public void Initialize() {
            _initializer.Initialize();
        }

    }
}