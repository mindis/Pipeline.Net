using System.Collections.Generic;
using System.Data.SqlClient;
using Pipeline.Extensions;
using Pipeline.Interfaces;
using Dapper;

namespace Pipeline.Provider.SqlServer {

    /// <summary>
    /// Writes one query to read all the keys, 
    /// then writes many queries to read the data based on the keys.
    /// Use this if reading the data with SqlInputReader causes blocking 
    /// for other applications.  It is slower, and requires the creation of
    /// temporary tables, but it reads according to the batch-size set on the 
    /// connection and is less likely to block (the smaller the batch size).  Of course, 
    /// you can also set no-lock on the entity to reduce blocking, but at the risk of 
    /// reading un-commited data.
    /// </summary>
    public class SqlInputBatchReader : IReadInput {
        readonly InputContext _input;
        readonly IReadInput _reader;
        readonly SqlEntityMatchingFieldsReader _fieldsReader;
        int _rowCount;

        public SqlInputBatchReader(InputContext input, IReadInput reader) {
            _input = input;
            _reader = reader;
            _fieldsReader = new SqlEntityMatchingFieldsReader(input);
        }

        public IEnumerable<Row> Read() {
            using (var cn = new SqlConnection(_input.Connection.GetConnectionString())) {
                cn.Open();
                foreach (var batch in _reader.Read().Partition(_input.Entity.ReadSize)) {
                    foreach(var row in _fieldsReader.Read(batch)) {
                        _rowCount++;
                        _input.Increment();
                        yield return row;
                    }
                }
            }
            _input.Info("{0} from {1}", _rowCount, _input.Connection.Name);
        }

        public void LoadVersion() {
            using (var cn = new SqlConnection(_input.Connection.GetConnectionString())) {
                cn.Open();
                _input.Entity.MaxVersion = _input.Entity.Version == string.Empty ? null : cn.ExecuteScalar(_input.SqlGetInputMaxVersion());
            }
        }
    }
}