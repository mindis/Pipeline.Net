using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {
    public class SqlEntityInitializer : IEntityInitializer {

        private readonly PipelineContext _context;
        private readonly string _connectionString;

        public SqlEntityInitializer(PipelineContext context) {
            _context = context;
            _connectionString = _context.Process.Connections.First(c => c.Name == "output").GetConnectionString();
        }

        private void Destroy(IDbConnection cn) {
            try {
                cn.Execute(_context.SqlDropOutputViewStatement());
            } catch { }

            try {
                cn.Execute(_context.SqlDropControlStatement());
            } catch { }

            try {
                cn.Execute(_context.SqlDropOutputStatement());
            } catch { }
        }

        private void Create(IDbConnection cn) {
            cn.Execute(_context.SqlCreateOutputStatement());
            cn.Execute(_context.SqlCreateOutputUniqueClusteredIndex());
            cn.Execute(_context.SqlCreateOutputPrimaryKey());
            cn.Execute(_context.SqlCreateOutputViewStatement());
            cn.Execute(_context.SqlCreateControlStatement());
        }

        public void Initialize() {
            using (var cn = new SqlConnection(_connectionString)) {
                cn.Open();
                Destroy(cn);
                Create(cn);
            }
        }
    }
}
