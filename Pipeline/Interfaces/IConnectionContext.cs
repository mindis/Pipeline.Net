using Pipeline.Configuration;

namespace Pipeline.Interfaces {
    public interface IConnectionContext : IContext, IIncrement {
        Connection Connection { get; }
    }
}
