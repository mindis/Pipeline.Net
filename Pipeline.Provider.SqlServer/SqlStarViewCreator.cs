using System.Data.SqlClient;
using Dapper;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {
    public class SqlStarViewCreator : IAction {
        private readonly OutputContext _output;

        public SqlStarViewCreator(OutputContext output) {
            _output = output;
        }

        public void Execute() {
            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();
                try {
                    cn.Execute(_output.SqlDropStarView());
                } catch (SqlException) {
                }
                cn.Execute(_output.SqlCreateStarView());
            }
        }
    }
}