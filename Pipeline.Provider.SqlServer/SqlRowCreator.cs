using Pipeline.Configuration;
using Pipeline.Transformers;
using System;
using System.Collections.Generic;
using System.Data;

namespace Pipeline.Provider.SqlServer {
    public class SqlRowCreator {
        IContext _context;
        readonly HashSet<int> _errors;

        public SqlRowCreator(IContext context) {
            _errors = new HashSet<int>();
            _context = context;  
        }

        public Row Create(IDataReader reader, int rowCapacity, Field[] fields) {
            var row = new Row(rowCapacity, _context.Entity.IsMaster);
            for (var i = 0; i < reader.FieldCount; i++) {
                var field = fields[i];
                if (field.Type == "string") {
                    if (reader.GetFieldType(i) == typeof(string)) {
                        row.SetString(field, reader.IsDBNull(i) ? null : reader.GetString(i));
                    } else {
                        TypeMismatch(field, reader, i);
                        var value = reader[i];
                        row[field] = value == DBNull.Value ? null : value;
                    }
                } else {
                    var value = reader.GetValue(i);
                    row[field] = value == DBNull.Value ? null : value;
                }
            }
            return row;
        }

        public void TypeMismatch(Field field, IDataReader reader, int index) {
            var key = HashcodeTransform.GetHashCode(field.Name, field.Type);
            if (_errors.Add(key)) {
                _context.Error("Type mismatch for {0}. Expected {1}, but read {2}.", field.Name, field.Type, reader.GetFieldType(index));
            }
        }

    }
}
