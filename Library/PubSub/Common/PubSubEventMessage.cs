using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx
{
    public class PubSubEventMessage
    {
        public required string Arguments {get;set;}
        public required string TargetType {get;set;}
        public required string Method {get;set;}
        public required string Context {get;set;}
    }
}