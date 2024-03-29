using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline {

    public class OutputContext : IConnectionContext {
        readonly IIncrement _incrementer;
        readonly IContext _context;

        public Connection Connection { get; set; }
        public Field[] OutputFields { get; set; }

        public Entity Entity => _context.Entity;

        public Field Field => _context.Field;

        public Process Process => _context.Process;

        public Transform Transform => _context.Transform;

        public OutputContext(PipelineContext context, IIncrement incrementer) {
            context.Activity = PipelineActivity.Load;
            _context = context;
            _incrementer = incrementer;
            OutputFields = context.GetAllEntityOutputFields().ToArray();
            Connection = context.Process.Connections.First(c => c.Name == "output");
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