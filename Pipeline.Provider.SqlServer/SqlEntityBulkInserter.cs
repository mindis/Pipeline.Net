using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Extensions;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityBulkInserter : BaseEntityWriter, IEntityWriter {
        private SqlBulkCopyOptions _bulkCopyOptions;

        public SqlEntityBulkInserter(PipelineContext context)
            : base(context) {
            _bulkCopyOptions = SqlBulkCopyOptions.Default;

            TurnOptionOn(SqlBulkCopyOptions.TableLock);
            TurnOptionOn(SqlBulkCopyOptions.UseInternalTransaction);
            TurnOptionOff(SqlBulkCopyOptions.CheckConstraints);
            TurnOptionOff(SqlBulkCopyOptions.FireTriggers);
            TurnOptionOn(SqlBulkCopyOptions.KeepNulls);
        }

        private void TurnOptionOn(SqlBulkCopyOptions option) {
            _bulkCopyOptions |= option;
        }
        private bool IsOptionOn(SqlBulkCopyOptions option) {
            return (_bulkCopyOptions & option) == option;
        }

        private void TurnOptionOff(SqlBulkCopyOptions option) {
            if (IsOptionOn(option))
                _bulkCopyOptions ^= option;
        }

        public void Write(IEnumerable<Row> rows, int batchId) {

            using (var cn = new SqlConnection(Connection.GetConnectionString())) {
                cn.Open();
                var count = 0;
                SqlDataAdapter adapter;
                var dt = new DataTable();
                using (adapter = new SqlDataAdapter(Context.SqlSelectOutputSchema(), cn)) {
                    adapter.Fill(dt);
                }

                var bulkCopy = new SqlBulkCopy(cn, _bulkCopyOptions, null) {
                    BatchSize = Connection.BatchSize,
                    BulkCopyTimeout = Connection.Timeout,
                    DestinationTableName = Context.SqlOutputTableName(),
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
                        dr.ItemArray = row.ToObjectArray(OutputFields, batchId);
                        dataRows.Add(dr);
                        batchCount++;
                    }

                    bulkCopy.WriteToServer(dataRows.ToArray());

                    Increment(batchCount);
                    count += batchCount;
                }
                Context.Info("{0} to {1}", count, Connection.Name);

            }
        }

        public object GetVersion() {
            if (Context.Entity.Version == string.Empty)
                return null;

            var field = Context.Entity.GetVersionField();

            if (field == null)
                return null;

            using (var cn = new SqlConnection(Connection.GetConnectionString())) {
                return cn.ExecuteScalar(Context.SqlGetOutputMaxVersion(field));
            }
        }
    }
}