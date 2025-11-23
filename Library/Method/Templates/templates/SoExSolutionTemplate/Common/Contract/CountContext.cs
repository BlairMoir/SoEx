using Newtonsoft.Json;

namespace SoExTemplate.Common.Contract
{
    public readonly struct CountContext
    {
        public CountContext() { }

        public CountContext(CountContext parent)
        {
            HopCount = parent.HopCount + 1;
        }

        public int HopCount { get; init; } = 1;
    }
}
