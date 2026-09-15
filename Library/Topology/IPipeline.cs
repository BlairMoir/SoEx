using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SoEx.Abstractions;
using SoEx.Topology.Pipeline;

namespace SoEx.Topology
{
    public interface IPipeline : IBindingPipeline
    {


        public IPipelineConfidentiality TelemetryConfidentiality { get; }

        public Type[] ServiceInterceptors { get; }
    }
}
