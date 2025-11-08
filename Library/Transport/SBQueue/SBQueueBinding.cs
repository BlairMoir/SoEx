using System.Diagnostics.CodeAnalysis;

namespace SoEx.Transport.SBQueue
{
    public class SBQueueBinding<I> : Topology.Binding
    {
        private readonly SBConfig _config;
        [SetsRequiredMembers]
        public SBQueueBinding(SBConfig config)
        {
            Contract = typeof(I);
            SubSystem = "NotSet";
            Transport = new SBQueueTransport() { Address = new Uri($"sb://{config.SBNamespace}.servicebus.windows.net/") };
            _config = config;
        }

        public SBConfig Config => _config;
    }
}
