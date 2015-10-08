using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Pipeline.Extensions;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {

    public class SqlEntityBulkInserter : IWrite {

        SqlBulkCopyOptions _bulkCopyOptions;
        readonly OutputContext _output;
        readonly SqlEntityMatchingKeysReader _outputKeysReader;
        readonly SqlEntityUpdater _sqlUpdater;
        readonly Field[] _keys;

        public SqlEntityBulkInserter(OutputContext output) {
            _output = output;
            _bulkCopyOptions = SqlBulkCopyOptions.Default;

            TurnOptionOn(SqlBulkCopyOptions.TableLock);
            TurnOptionOn(SqlBulkCopyOptions.UseInternalTransaction);
            TurnOptionOff(SqlBulkCopyOptions.CheckConstraints);
            TurnOptionOff(SqlBulkCopyOptions.FireTriggers);
            TurnOptionOn(SqlBulkCopyOptions.KeepNulls);

            _keys = output.Entity.GetPrimaryKey();
            _outputKeysReader = new SqlEntityMatchingKeysReader(output, _keys);
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

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();

                SqlDataAdapter adapter;
                var dt = new DataTable();
                using (adapter = new SqlDataAdapter(_output.SqlSelectOutputSchema(), cn)) {
                    adapter.Fill(dt);
                }

                var bulkCopy = new SqlBulkCopy(cn, _bulkCopyOptions, null) {
                    BatchSize = _output.Entity.InsertSize,
                    BulkCopyTimeout = _output.Connection.Timeout,
                    DestinationTableName = "[" + _output.Entity.OutputTableName(_output.Process.Name) + "]",
                };

                var counter = 0;
                for (int i = 0; i < _output.OutputFields.Length; i++) {
                    bulkCopy.ColumnMappings.Add(i, i);
                    counter++;
                }
                bulkCopy.ColumnMappings.Add(counter, counter); //TflBatchId

                foreach (var part in rows.Partition(_output.Entity.InsertSize)) {

                    var inserts = new List<DataRow>(_output.Entity.InsertSize);
                    var updates = new List<Row>();
                    var batchCount = 0;

                    if (firstRun) {
                        foreach (var row in part) {
                            inserts.Add(GetDataRow(dt, row));
                            batchCount++;
                        }
                    } else {
                        var matching = _outputKeysReader.Read(part);

                        var batch = part.ToArray();
                        for (int i = 0, batchLength = batch.Length; i < batchLength; i++) {
                            var row = batch[i];
                            var match = matching.FirstOrDefault(f => f.Match(_keys, row));
                            if (match == null) {
                                inserts.Add(GetDataRow(dt, row));
                            } else {
                                if (!match.TflHashCode.Equals(row.TflHashCode)) {
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
                        _output.Entity.Updates += updates.Count;
                    }

                    _output.Increment(batchCount);
                }

                _output.Info("{0} inserts into {1}", _output.Entity.Inserts, _output.Connection.Name);

                _output.Info("{0} updates to {1}", _output.Entity.Updates, _output.Connection.Name);
            }

        }

        DataRow GetDataRow(DataTable dataTable, Row row) {
            var dr = dataTable.NewRow();
            var values = new List<object>(row.ToEnumerable(_output.OutputFields)) { _output.Entity.BatchId };
            dr.ItemArray = values.ToArray();
            return dr;
        }

    }
}