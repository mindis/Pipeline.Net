using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityController : IEntityController {
        readonly OutputContext _context;
        readonly IInitializer _initializer;
        readonly Connection _output;

        public object StartVersion { get; private set; }

        public SqlEntityController(OutputContext context, IInitializer initializer) {
            _context = context;
            _initializer = initializer;
            _output = context.Process.Connections.First(c => c.Name == "output");
            StartVersion = null;
        }

        int GetBatchId(IDbConnection cn) {
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
                cn.Execute(_context.SqlControlEndBatch(), new {
                    _context.Entity.Inserts,
                    _context.Entity.Updates,
                    _context.Entity.Deletes,
                    _context.Entity.BatchId
                });
            }
        }

        public void Initialize() {
            _initializer.Initialize();
        }

    }
}