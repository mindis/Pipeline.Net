using System;
using System.Collections.Generic;
using Pipeline.Interfaces;

namespace Pipeline.Command {
    public class ConsoleWriter : IWrite {
        public void Write(IEnumerable<Row> rows) {
            foreach (var row in rows) {
                Console.WriteLine(row);
            }
        }
    }
}