using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Logging;

namespace Pipeline.Provider.SqlServer {

    public class BaseSqlEntityReader : BaseEntityReader {

        public BaseSqlEntityReader(Process process, Entity entity, IPipelineLogger logger)
            : base(process, entity, logger) {
            Connection = process.Connections.First(c => c.Name == entity.Connection);
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

            var cs = builder.ConnectionString;
            Logger.Debug(Context, cs);
            return cs;
        }
    }
}