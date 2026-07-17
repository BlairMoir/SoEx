using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SoEx.Abstractions;

namespace SoEx.Topology
{
    public interface IPipeline
    {
        public Type Dispatcher { get; }
        public Type MessageSerializer { get; }
        public Type TelemetryConfidentiality { get; }
        public Type MessageProtection { get; }
        public Type[] ServiceInterceptors { get; }
        public Type[] KnownTypes { get; }
    }
}
