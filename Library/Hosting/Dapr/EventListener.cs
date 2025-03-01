using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx.Dapr
{
    public class EventListener
    {
        public required Type EventService {get; init;}
        public required Type EventInterface {get; init;}
    }
}