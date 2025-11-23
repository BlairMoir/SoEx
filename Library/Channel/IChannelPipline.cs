using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Channel
{
    public interface IChannelPipeline
    {
        public Task<InvocationResponse> ClientPipeLine(InvocationRequest request, IChannel channel, Activity? parentActivity);
    }
}
