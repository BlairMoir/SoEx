using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx.Transport.ASBTopic
{
    public static class ASBTopicSubject
    {
        public static string For<I>()
        {
            return typeof(I).Name;
        }
    }
}
