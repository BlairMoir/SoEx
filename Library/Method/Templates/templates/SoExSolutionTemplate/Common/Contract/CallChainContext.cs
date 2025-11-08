using Newtonsoft.Json;

namespace SoExTemplate.Common.Contract
{
    public class CallChainContext
    {
        private readonly Guid _callChainId;

        public CallChainContext()
        {
            _callChainId = Guid.NewGuid();
        }

        [JsonConstructor]
        public CallChainContext(Guid CallChainId)
        {
            _callChainId = CallChainId;
        }

        public Guid CallChainId => _callChainId;
    }
}
