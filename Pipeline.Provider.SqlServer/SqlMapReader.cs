using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {
   public class SqlMapReader : IMapReader {
      public IEnumerable<MapItem> Read(PipelineContext context) {
         var items = new List<MapItem>();
         var map = context.Process.Maps.First(m => m.Name == context.Transform.Map);
         var connection = context.Process.Connections.First(cn => cn.Name == map.Connection);
         using (var cn = new SqlConnection(connection.GetConnectionString())) {
            cn.Open();
            items.AddRange(cn.Query<MapItem>(map.Query, commandTimeout: connection.Timeout, commandType: System.Data.CommandType.Text));
         }
         return items;
      }
   }
}
