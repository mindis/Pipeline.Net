using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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

        public void Write(IEnumerable<Row> rows) {

            var batchId = 0;

            using (var cn = new SqlConnection(Connection.GetConnectionString())) {
                cn.Open();

                try {
                    cn.Execute(Context.SqlDropOutputStatement());
                } catch { } finally {
                    cn.Execute(Context.SqlCreateOutputStatement());
                    cn.Execute(Context.SqlCreateOutputUniqueClusteredIndex());
                    cn.Execute(Context.SqlCreateOutputPrimaryKey());
                }

                var count = 0;
                SqlDataAdapter adapter;
                var dt = new DataTable();
                using (adapter = new SqlDataAdapter(Context.SqlSelectOutputSchemaStatement(), cn)) {
                    adapter.Fill(dt);
                }

                var bulkCopy = new SqlBulkCopy(cn, _bulkCopyOptions, null) {
                    BatchSize = Connection.BatchSize,
                    BulkCopyTimeout = Connection.Timeout,
                    DestinationTableName = Context.Entity.OutputName(Context.Process.Name),
                };

                var counter = 0;
                for (int i = 0; i < OutputFields.Length; i++) {
                    bulkCopy.ColumnMappings.Add(i,i);
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
    }
}