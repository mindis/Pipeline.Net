using System.Linq;
using Autofac;
using Pipeline.Configuration;
using Pipeline.Interfaces;
using Pipeline.Provider.SqlServer;

namespace Pipeline.Command.Modules {
    public class MapModule : Module {
        readonly Root _root;

        public MapModule(Root root) {
            _root = root;
        }

        protected override void Load(ContainerBuilder builder) {
            foreach (var process in _root.Processes) {
                foreach (var m in process.Maps) {
                    var map = m;
                    builder.Register<IMapReader>(ctx => {
                        var connection = process.Connections.FirstOrDefault(cn => cn.Name == map.Connection);
                        var provider = connection == null ? string.Empty : connection.Provider;
                        switch (provider) {
                            case "sqlserver":
                                return new SqlMapReader();
                            default:
                                return new DefaultMapReader();
                        }
                    }).Named<IMapReader>(map.Key);
                }
            }
        }
    }
}