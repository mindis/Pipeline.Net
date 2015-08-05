using Pipeline.Configuration;

namespace Pipeline {
    public interface IConnectionContext : IContext, IIncrement {
        Connection Connection { get; }
    }
}
