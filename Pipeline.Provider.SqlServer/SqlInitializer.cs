﻿using Dapper;
using Pipeline.Interfaces;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Pipeline.Provider.SqlServer {
    public class SqlInitializer : IInitializer {
        OutputContext _context;
        readonly string _connectionString;

        public SqlInitializer(OutputContext context) {
            _context = context;
            _connectionString = _context.Process.Connections.First(c => c.Name == "output").GetConnectionString();
        }

        void Destroy(IDbConnection cn) {
            try {
                cn.Execute(_context.SqlDropControl());
            } catch { }
        }

        void Create(IDbConnection cn) {
            cn.Execute(_context.SqlCreateControl());
        }

        public void Initialize() {
            using (var cn = new SqlConnection(_connectionString)) {
                cn.Open();
                Destroy(cn);
                Create(cn);
            }

        }
    }

}
