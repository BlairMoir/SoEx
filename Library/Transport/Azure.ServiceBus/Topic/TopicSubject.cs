using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx.Transport.Azure.ServiceBus
{
    public static class TopicSubject
    {
        public static string For<I>()
        {
            return typeof(I).Name;
        }
    }
}
