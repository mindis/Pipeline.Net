using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Configuration;
using Pipeline.Extensions;
using System.Linq;

namespace Pipeline.Provider.SqlServer {

   public class SqlEntityBulkInserter : BaseEntityWriter, IEntityWriter {
      SqlBulkCopyOptions _bulkCopyOptions;

      public SqlEntityBulkInserter(PipelineContext context)
            : base(context) {
         _bulkCopyOptions = SqlBulkCopyOptions.Default;

         TurnOptionOn(SqlBulkCopyOptions.TableLock);
         TurnOptionOn(SqlBulkCopyOptions.UseInternalTransaction);
         TurnOptionOff(SqlBulkCopyOptions.CheckConstraints);
         TurnOptionOff(SqlBulkCopyOptions.FireTriggers);
         TurnOptionOn(SqlBulkCopyOptions.KeepNulls);

      }

      void TurnOptionOn(SqlBulkCopyOptions option) {
         _bulkCopyOptions |= option;
      }
      bool IsOptionOn(SqlBulkCopyOptions option) {
         return (_bulkCopyOptions & option) == option;
      }

      void TurnOptionOff(SqlBulkCopyOptions option) {
         if (IsOptionOn(option))
            _bulkCopyOptions ^= option;
      }

      public int Write(IEnumerable<Row> rows) {
         var count = 0;

         using (var cn = new SqlConnection(Connection.GetConnectionString())) {
            cn.Open();

            SqlDataAdapter adapter;
            var dt = new DataTable();
            using (adapter = new SqlDataAdapter(Context.SqlSelectOutputSchema(), cn)) {
               adapter.Fill(dt);
            }

            var bulkCopy = new SqlBulkCopy(cn, _bulkCopyOptions, null) {
               BatchSize = Connection.BatchSize,
               BulkCopyTimeout = Connection.Timeout,
               DestinationTableName = "[" + Context.Entity.OutputTableName(Context.Process.Name) + "]",
            };

            var counter = 0;
            for (int i = 0; i < OutputFields.Length; i++) {
               bulkCopy.ColumnMappings.Add(i, i);
               counter++;
            }
            bulkCopy.ColumnMappings.Add(counter, counter); //TflBatchId

            foreach (var batch in rows.Partition(Connection.BatchSize)) {

               var dataRows = new List<DataRow>(Connection.BatchSize);
               var batchCount = 0;
               foreach (var row in batch) {
                  var dr = dt.NewRow();
                  var values = new List<object>(row.ToEnumerable(OutputFields)) { Context.Entity.BatchId };
                  dr.ItemArray = values.ToArray();
                  dataRows.Add(dr);
                  batchCount++;
               }

               bulkCopy.WriteToServer(dataRows.ToArray());

               Increment(batchCount);
               count += batchCount;
            }
            Context.Info("{0} to {1}", count, Connection.Name);

         }
         return count;
         Context.Entity.Inserts = count;
      }

      public void LoadVersion() {
         if (Context.Entity.Version == string.Empty)
            return;

         var field = Context.Entity.GetVersionField();

         if (field == null)
            return;

         using (var cn = new SqlConnection(Connection.GetConnectionString())) {
            Context.Entity.MinVersion = cn.ExecuteScalar(Context.SqlGetOutputMaxVersion(field));
         }
      }
   }
}