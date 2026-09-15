using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SoEx.Topology;

namespace SoEx.Endpoint
{
    public interface IEndpointPipeline
    {
        public Task<byte[]> ServicePipeLine<I>(byte[] payload, IBindingPipeline? pipeline, Activity? parentActivity) where I : class;
    }
}
