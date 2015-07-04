using System.Data.SqlClient;
using Pipeline.Configuration;

namespace Pipeline.Provider.SqlServer {

    public class BaseSqlEntityReader : BaseEntityReader {
        public BaseSqlEntityReader(Entity entity, Connection connection)
            : base(entity) {
            Connection = connection;
        }

        public Connection Connection { get; set; }

        public string GetConnectionString() {
            if (Connection.ConnectionString != string.Empty) {
                return Connection.ConnectionString;
            }

            var builder = new SqlConnectionStringBuilder {
                ApplicationName = Constants.ApplicationName,
                ConnectTimeout = Connection.Timeout,
                DataSource = Connection.Server,
                InitialCatalog = Connection.Database,
                IntegratedSecurity = Connection.User == string.Empty,
                UserID = Connection.User,
                Password = Connection.Password
            };

            return builder.ConnectionString;
        }
    }
}