using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx.Transport.NATS
{
    public static class NatsSubject
    {
        public static string For<I>()
        {
            return typeof(I).Name;
        }
    }
}
