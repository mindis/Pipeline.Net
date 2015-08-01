using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Configuration;
using Pipeline.Extensions;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityBulkInserter : IWrite {

        SqlBulkCopyOptions _bulkCopyOptions;
        EntityOutput _output;

        public SqlEntityBulkInserter(EntityOutput output) {
            _output = output;
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

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();

                SqlDataAdapter adapter;
                var dt = new DataTable();
                using (adapter = new SqlDataAdapter(_output.Context.SqlSelectOutputSchema(), cn)) {
                    adapter.Fill(dt);
                }

                var bulkCopy = new SqlBulkCopy(cn, _bulkCopyOptions, null) {
                    BatchSize = _output.Connection.BatchSize,
                    BulkCopyTimeout = _output.Connection.Timeout,
                    DestinationTableName = "[" + _output.Context.Entity.OutputTableName(_output.Context.Process.Name) + "]",
                };

                var counter = 0;
                for (int i = 0; i < _output.OutputFields.Length; i++) {
                    bulkCopy.ColumnMappings.Add(i, i);
                    counter++;
                }
                bulkCopy.ColumnMappings.Add(counter, counter); //TflBatchId

                foreach (var batch in rows.Partition(_output.Connection.BatchSize)) {

                    var dataRows = new List<DataRow>(_output.Connection.BatchSize);
                    var batchCount = 0;
                    foreach (var row in batch) {
                        var dr = dt.NewRow();
                        var values = new List<object>(row.ToEnumerable(_output.OutputFields)) { _output.Context.Entity.BatchId };
                        dr.ItemArray = values.ToArray();
                        dataRows.Add(dr);
                        batchCount++;
                    }

                    bulkCopy.WriteToServer(dataRows.ToArray());

                    _output.Increment(batchCount);
                    count += batchCount;
                }
                _output.Context.Info("{0} to {1}", count, _output.Connection.Name);

            }
            return count;
            _output.Context.Entity.Inserts = count;
        }

        public void LoadVersion() {
            if (_output.Context.Entity.Version == string.Empty)
                return;

            var field = _output.Context.Entity.GetVersionField();

            if (field == null)
                return;

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                _output.Context.Entity.MinVersion = cn.ExecuteScalar(_output.Context.SqlGetOutputMaxVersion(field));
            }
        }

    }
}