using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Configuration;
using System.Diagnostics;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityController : IEntityController {

        readonly Stopwatch _stopWatch;
        readonly OutputContext _context;
        readonly IInitializer _initializer;
        readonly Connection _output;

        public object StartVersion { get; private set; }

        public SqlEntityController(OutputContext context, IInitializer initializer) {
            _context = context;
            _initializer = initializer;
            _output = context.Process.Connections.First(c => c.Name == "output");
            StartVersion = null;
            _stopWatch = new Stopwatch();
        }

        int GetBatchId(IDbConnection cn) {
            var batchId = cn.Query<int>(_context.SqlControlLastBatchId()).FirstOrDefault() + 1;
            _context.Info("Batch " + batchId);
            return batchId;
        }

        public void Start() {
            _stopWatch.Start();
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
            _stopWatch.Stop();
            _context.Info("Time elaspsed {0}", _stopWatch.Elapsed);
        }

        public void Initialize() {
            _initializer.Initialize();
        }

    }
}