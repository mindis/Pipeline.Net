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
        readonly IAction _initializer;

        public object StartVersion { get; private set; }

        public SqlEntityController(OutputContext context, IAction initializer) {
            _context = context;
            _initializer = initializer;
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

            // load input version
            using (var cn = new SqlConnection(_context.Process.Connections.First(c => c.Name == _context.Entity.Connection).GetConnectionString())) {
                cn.Open();
                _context.Entity.MaxVersion = _context.Entity.Version == string.Empty ? null : cn.ExecuteScalar(_context.SqlGetInputMaxVersion());
            }

            // load control batch
            using (var cn = new SqlConnection(_context.Connection.GetConnectionString())) {
                cn.Open();
                _context.Entity.BatchId = GetBatchId(cn);
                cn.Execute(_context.SqlControlStartBatch(), new { _context.Entity.BatchId, Entity = _context.Entity.Alias });

                // load output version
                if (_context.Entity.Version == string.Empty)
                    return;

                var field = _context.Entity.GetVersionField();

                if (field == null)
                    return;

                _context.Entity.MinVersion = cn.ExecuteScalar(_context.SqlGetOutputMaxVersion(field));
            }

        }

        public void End() {
            using (var cn = new SqlConnection(_context.Connection.GetConnectionString())) {
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
            _initializer.Execute();
        }

    }
}