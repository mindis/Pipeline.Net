using System;
using System.Collections.Generic;
using Pipeline.Configuration;

namespace Pipeline {
    public interface IContext {
        Process Process { get; }
        Entity Entity { get; }
        Field Field { get; }
        Transform Transform { get; }
        void Debug(string message, params object[] args);
        void Error(string message, params object[] args);
        void Error(Exception exception, string message, params object[] args);
        void Info(string message, params object[] args);
        void Warn(string message, params object[] args);
        IEnumerable<Field> GetAllEntityFields();
        IEnumerable<Field> GetAllEntityOutputFields();

    }
}