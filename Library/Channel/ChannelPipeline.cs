using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Exceptions;
using SoEx.Topology;

namespace SoEx.Channel
{
    public class ChannelPipeline : IChannelPipeline
    {
        readonly ILifetimeScope _scope;
        readonly ILogger<ChannelPipeline> _logger;

        public ChannelPipeline(ILogger<ChannelPipeline> logger, ILifetimeScope scope)
        {
            _scope = scope;
            _logger = logger;
        }

        public async Task<InvocationResponse> ClientPipeLine(InvocationRequest request, IChannel channel, Activity? parentActivity)
        {
            try
            {
                Debug.Assert(channel is not null);
                Type serializerType = channel.Pipeline?.MessageSerializer ?? typeof(IMessageSerializer);
                var serializer = (IMessageSerializer)_scope.Resolve(serializerType);
                var serializedRequest = serializer.Serialize(request);
                var response = await channel.InvokeResult(serializedRequest);

                if (response.Length == 0)
                {
                    return new InvocationResponse();
                }

                var deserializedResponse = serializer.Deserialize<InvocationResponse>(response);
                Debug.Assert(deserializedResponse is not null);
                return deserializedResponse;
            }
            catch (ServiceException)
            {
                throw new ClientException("Error during service invocation");
            }
            catch (Exception ex)
            {
                throw new ClientException("Error communicationg with service", ex);
            }
        }
    }
}
