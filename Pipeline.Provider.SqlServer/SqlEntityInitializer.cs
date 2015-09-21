using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityInitializer : IAction {

        readonly OutputContext _context;
        readonly string _connectionString;

        public SqlEntityInitializer(OutputContext context) {
            _context = context;
            _connectionString = _context.Process.Connections.First(c => c.Name == "output").GetConnectionString();
        }

        void Destroy(IDbConnection cn) {
            try {
                cn.Execute(_context.SqlDropOutputView());
            } catch { }

            try {
                cn.Execute(_context.SqlDropOutput());
            } catch { }
        }

        void Create(IDbConnection cn){ 
            cn.Execute(_context.SqlCreateOutput());
            cn.Execute(_context.SqlCreateOutputUniqueClusteredIndex());
            cn.Execute(_context.SqlCreateOutputPrimaryKey());
            cn.Execute(_context.SqlCreateOutputView());
        }

        public ActionResponse Execute() {
            using (var cn = new SqlConnection(_connectionString)) {
                cn.Open();
                Destroy(cn);
                Create(cn);
            }
            return new ActionResponse();
        }
    }
}
