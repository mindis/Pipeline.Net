using System;
using System.Collections.Generic;
using Pipeline.Configuration;

namespace Pipeline {
    public interface IContext {
        Entity Entity { get; }
        Field Field { get; }
        Process Process { get; }
        Transform Transform { get; }

        void Debug(string message, params object[] args);
        void Error(string message, params object[] args);
        void Error(Exception exception, string message, params object[] args);
        IEnumerable<Field> GetAllEntityFields();
        IEnumerable<Field> GetAllEntityOutputFields();
        void Info(string message, params object[] args);
        void Warn(string message, params object[] args);
    }
}