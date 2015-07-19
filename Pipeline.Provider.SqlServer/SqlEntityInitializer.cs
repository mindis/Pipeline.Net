using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

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
                cn.Execute(_context.SqlDropOutputView());
            } catch { }

            try {
                cn.Execute(_context.SqlDropControl());
            } catch { }

            try {
                cn.Execute(_context.SqlDropOutput());
            } catch { }
        }

        private void Create(IDbConnection cn) {
            cn.Execute(_context.SqlCreateOutput());
            cn.Execute(_context.SqlCreateOutputUniqueClusteredIndex());
            cn.Execute(_context.SqlCreateOutputPrimaryKey());
            cn.Execute(_context.SqlCreateOutputView());
            cn.Execute(_context.SqlCreateControl());
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
