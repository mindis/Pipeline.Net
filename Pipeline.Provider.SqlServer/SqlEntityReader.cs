using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Configuration;
using Pipeline.Transformers;

namespace Pipeline.Provider.SqlServer {

   public class SqlEntityReader : BaseEntityReader, IEntityReader {

      int _rowCount;
      HashSet<int> _errors = new HashSet<int>();

      public SqlEntityReader(PipelineContext context)
          : base(context) {

      }

      public IEnumerable<Row> Read() {
         using (var cn = new SqlConnection(Connection.GetConnectionString())) {

            cn.Open();
            var cmd = cn.CreateCommand();

            LoadVersion(cn);

            if (Context.Entity.MaxVersion == null) {
               cmd.CommandText = Context.SqlSelectInput();
            } else {
               if (Context.Entity.MinVersion == null) {
                  cmd.CommandText = Context.SqlSelectInputWithMaxVersion();
                  cmd.Parameters.AddWithValue("@Version", Context.Entity.MaxVersion);
               } else {
                  if (Context.Entity.MinVersion == Context.Entity.MaxVersion) {
                     yield break;
                  }
                  cmd.CommandText = Context.SqlSelectInputWithMinAndMaxVersion();
                  cmd.Parameters.AddWithValue("@MinVersion", Context.Entity.MinVersion);
                  cmd.Parameters.AddWithValue("@MaxVersion", Context.Entity.MaxVersion);
               }
            }

            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = Connection.Timeout;

            var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read()) {
               _rowCount++;
               var row = new Row(RowCapacity, Context.Entity.IsMaster);
               for (var i = 0; i < reader.FieldCount; i++) {
                  var field = InputFields[i];
                  if (field.Type == "string") {
                     if (reader.GetFieldType(i) == typeof(string)) {
                        row.SetString(field, reader.IsDBNull(i) ? null : reader.GetString(i));
                     } else {
                        TypeMismatch(field, reader, i);
                        var value = reader[i];
                        row[field] = value == DBNull.Value ? null : value;
                     }
                  } else {
                     var value = reader[i];
                     row[field] = value == DBNull.Value ? null : value;
                  }
               }

               Increment();
               yield return row;
            }

            Context.Info("{0} from {1}", _rowCount, Connection.Name);
         }
      }

      public void TypeMismatch(Field field, SqlDataReader reader, int index) {
         var key = HashcodeTransform.GetHashCode(field.Name, field.Type);
         if (_errors.Add(key)) {
            Context.Error("Type mismatch for {0}. Expected {1}, but read {2}.", field.Name, field.Type, reader.GetFieldType(index));
         }
      }

      void LoadVersion(IDbConnection cn) {
         Context.Entity.MaxVersion = Context.Entity.Version == string.Empty ? null : cn.ExecuteScalar(Context.SqlGetInputMaxVersion());
      }
   }
}