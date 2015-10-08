using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Pipeline.Configuration;
using Pipeline.Extensions;
using Pipeline.Interfaces;

namespace Pipeline.Provider.SqlServer {
    public class SqlCalculatedFieldUpdater : IWrite {
        private readonly OutputContext _context;
        private readonly Process _original;

        public SqlCalculatedFieldUpdater(OutputContext context, Process original) {
            _context = context;
            _original = original;
        }

        public void Write(IEnumerable<Row> rows) {
            var sql = _context.SqlUpdateCalculatedFields(_original);
            var temp = new List<Field> { _context.Entity.Fields.First(f => f.Name == Constants.TflKey) };
            temp.AddRange(_context.Entity.CalculatedFields.Where(f => f.Output && f.Name != Constants.TflHashCode));
            var fields = temp.ToArray();

            var count = 0;
            using (var cn = new SqlConnection(_context.Connection.GetConnectionString())) {
                cn.Open();
                foreach (var batch in rows.Partition(_context.Entity.UpdateSize)) {
                    var trans = cn.BeginTransaction();
                    var batchCount = cn.Execute(
                        sql,
                        batch.Select(r => r.ToExpandoObject(fields)),
                        trans,
                        _context.Connection.Timeout,
                        CommandType.Text
                    );
                    trans.Commit();
                    count += batchCount;
                    _context.Increment(batchCount);
                }
                _context.Info("{0} to {1}", count, _context.Connection.Name);
            }
            _context.Entity.Updates += count;
        }
    }
}