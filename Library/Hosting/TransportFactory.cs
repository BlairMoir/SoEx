using Autofac;
using SoEx.Topology;

namespace SoEx.Hosting
{
    public class TransportFactory
    {
        readonly IEnumerable<Topology.Client> _clients;
        readonly ILifetimeScope _scope;
        public TransportFactory(IEnumerable<Topology.Client> clients, ILifetimeScope scope)
        {
            _clients = clients;
            _scope = scope;
        }
        public object Client(Type serviceInterface)
        {
            Topology.Client client = _clients.Single(s => s.GetType().GenericTypeArguments.Length == 1 && s.GetType().GenericTypeArguments[0] == serviceInterface);
            Topology.Binding binding = client.Service;
            Type channelType = binding.Transport.ClientChannel ?? throw new ArgumentException("Channel not defined on binding");
            object channel = _scope.Resolve(channelType.MakeGenericType(serviceInterface));
            ((IChannel)channel).Bind(binding);
            return channel;
        }
    }
}
