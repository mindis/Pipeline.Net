using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Extensions;
using System.Linq;
using Pipeline.Configuration;
using System;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityBulkInserter : IWrite {

        SqlBulkCopyOptions _bulkCopyOptions;
        readonly OutputContext _output;
        readonly SqlEntityOutputKeysReader _outputKeysReader;
        readonly Field _hashCode;
        readonly SqlEntityUpdater _sqlUpdater;

        public SqlEntityBulkInserter(OutputContext output) {
            _output = output;
            _bulkCopyOptions = SqlBulkCopyOptions.Default;

            TurnOptionOn(SqlBulkCopyOptions.TableLock);
            TurnOptionOn(SqlBulkCopyOptions.UseInternalTransaction);
            TurnOptionOff(SqlBulkCopyOptions.CheckConstraints);
            TurnOptionOff(SqlBulkCopyOptions.FireTriggers);
            TurnOptionOn(SqlBulkCopyOptions.KeepNulls);

            _outputKeysReader = new SqlEntityOutputKeysReader(output);
            _hashCode = output.Entity.CalculatedFields.First(f => f.Name == Constants.TflHashCode);
            _sqlUpdater = new SqlEntityUpdater(output);
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

        public void Write(IEnumerable<Row> rows) {
            var firstRun = _output.Entity.IsFirstRun();
            var count = 0;

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();

                SqlDataAdapter adapter;
                var dt = new DataTable();
                using (adapter = new SqlDataAdapter(_output.SqlSelectOutputSchema(), cn)) {
                    adapter.Fill(dt);
                }

                var bulkCopy = new SqlBulkCopy(cn, _bulkCopyOptions, null) {
                    BatchSize = _output.Connection.BatchSize,
                    BulkCopyTimeout = _output.Connection.Timeout,
                    DestinationTableName = "[" + _output.Entity.OutputTableName(_output.Process.Name) + "]",
                };

                var counter = 0;
                for (int i = 0; i < _output.OutputFields.Length; i++) {
                    bulkCopy.ColumnMappings.Add(i, i);
                    counter++;
                }
                bulkCopy.ColumnMappings.Add(counter, counter); //TflBatchId

                foreach (var batch in rows.Partition(_output.Connection.BatchSize)) {

                    var inserts = new List<DataRow>(_output.Connection.BatchSize);
                    var updates = new List<Row>();
                    var batchCount = 0;

                    if (firstRun) {
                        foreach (var row in batch) {
                            inserts.Add(GetDataRow(dt, row));
                            batchCount++;
                        }
                    } else {
                        var output = _outputKeysReader.Read(batch);
                        foreach (var row in batch) {
                            var existing = output[batchCount][_hashCode];
                            if (existing.Equals(0)) {  // insert
                                inserts.Add(GetDataRow(dt, row));
                            } else {
                                if (!existing.Equals(row[_hashCode])) { //update
                                    updates.Add(row);
                                }
                            }
                            batchCount++;
                        }
                    }

                    if (inserts.Any()) {
                        bulkCopy.WriteToServer(inserts.ToArray());
                        _output.Entity.Inserts += inserts.Count;
                    }

                    if (updates.Any()) {
                        _sqlUpdater.Write(updates);
                    }

                    _output.Increment(batchCount);
                    count += batchCount;
                }
                _output.Info("{0} to {1}", count, _output.Connection.Name);

            }

        }

        DataRow GetDataRow(DataTable dataTable, Row row) {
            var dr = dataTable.NewRow();
            var values = new List<object>(row.ToEnumerable(_output.OutputFields)) { _output.Entity.BatchId };
            dr.ItemArray = values.ToArray();
            return dr;
        }

        public void LoadVersion() {
            if (_output.Entity.Version == string.Empty)
                return;

            var field = _output.Entity.GetVersionField();

            if (field == null)
                return;

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                _output.Entity.MinVersion = cn.ExecuteScalar(_output.SqlGetOutputMaxVersion(field));
            }
        }

    }
}