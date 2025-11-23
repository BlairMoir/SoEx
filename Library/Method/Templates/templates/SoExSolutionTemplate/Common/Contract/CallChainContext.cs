using Newtonsoft.Json;

namespace SoExTemplate.Common.Contract
{
    public readonly struct CallChainContext
    {
        public CallChainContext()
        {
        }

        public Guid CallChainId { get; init; } = Guid.NewGuid();
    }
}
