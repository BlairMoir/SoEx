using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx.Relay
{
    public class RelayConfig
    {
        public required string RelayNamespace { get; init; }
        public required string ConnectionName { get; init; }
        public required string KeyName { get; init; }
        public required string Key { get; init; }
    }
}
