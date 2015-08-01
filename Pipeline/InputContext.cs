using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;

namespace Pipeline {
    public class InputContext : IIncrement, IContext {
        PipelineContext _context;
        IIncrement _incrementer;

        public Connection Connection { get; set; }

        public int RowCapacity { get; set; }
        public Field[] InputFields { get; set; }

        public Entity Entity {
            get {
                return _context.Entity;
            }
        }

        public Field Field {
            get {
                return _context.Field;
            }
        }

        public Process Process {
            get {
                return _context.Process;
            }
        }

        public Transform Transform {
            get {
                return _context.Transform;
            }
        }

        public InputContext(PipelineContext context, IIncrement incrementer) {
            _incrementer = incrementer;
            _context = context;
            _context.Activity = PipelineActivity.Extract;
            RowCapacity = context.GetAllEntityFields().Count();
            InputFields = context.Entity.Fields.Where(f => f.Input).ToArray();
            Connection = context.Process.Connections.First(c => c.Name == context.Entity.Connection);
        }

        public void Increment(int by = 1) {
            _incrementer.Increment(by);
        }

        public void Debug(string message, params object[] args) {
            _context.Debug(message, args);
        }

        public void Error(string message, params object[] args) {
            _context.Error(message, args);
        }

        public void Error(Exception exception, string message, params object[] args) {
            _context.Error(exception, message, args);
        }

        public IEnumerable<Field> GetAllEntityFields() {
            return _context.GetAllEntityFields();
        }

        public IEnumerable<Field> GetAllEntityOutputFields() {
            return _context.GetAllEntityOutputFields();
        }

        public void Info(string message, params object[] args) {
            _context.Info(message, args);
        }

        public void Warn(string message, params object[] args) {
            _context.Warn(message, args);
        }
    }
}
