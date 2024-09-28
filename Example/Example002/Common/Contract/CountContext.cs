using Newtonsoft.Json;

namespace Example002.Common.Contract
{
    public class CountContext
    {
        private int _hopCount = 1;

        public CountContext()
        {

        }

        public CountContext(CountContext parent)
        {
            _hopCount = parent.HopCount + 1;
        }

        [JsonConstructor]
        public CountContext(int HopCount)
        {
            _hopCount = HopCount;
        }

        public int HopCount => _hopCount;
    }
}
