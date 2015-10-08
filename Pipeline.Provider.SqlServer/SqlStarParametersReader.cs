using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {
    public class SqlStarParametersReader : IRead {
        private readonly OutputContext _output;
        private readonly Process _process;
        private readonly SqlRowCreator _rowCreator;

        public SqlStarParametersReader(OutputContext output, Process process) {
            _output = output;
            _process = process;
            _rowCreator = new SqlRowCreator(output);
        }

        public IEnumerable<Row> Read() {

            var batches = _process.Entities.Select(e => e.BatchId).ToArray();
            var minBatchId = batches.Min();
            var maxBatchId = batches.Max();
            _output.Info("Batch Range: {0} to {1}.", minBatchId, maxBatchId);

            var sql = $"SELECT {string.Join(",", _output.Entity.Fields.Select(f => "[" + f.Alias + "]"))} FROM [{_output.Process.Star}] WITH (NOLOCK) WHERE [TflBatchId] BETWEEN @MinBatchId AND @MaxBatchId;";
            _output.Debug(sql);

            using (var cn = new SqlConnection(_output.Connection.GetConnectionString())) {
                cn.Open();
                var cmd = cn.CreateCommand();

                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@MinBatchId", minBatchId);
                cmd.Parameters.AddWithValue("@MaxBatchId", maxBatchId);

                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                var rowCount = 0;
                var fieldArray = _output.Entity.Fields.ToArray();
                while (reader.Read()) {
                    rowCount++;
                    yield return _rowCreator.Create(reader, fieldArray.Length + _output.Entity.CalculatedFields.Count, fieldArray);
                }
                _output.Info("{0} from {1}", rowCount, _output.Connection.Name);
            }
        }
    }
}