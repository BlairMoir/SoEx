using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Abstractions.Protection;
using SoEx.Exceptions;
using SoEx.Topology;

namespace SoEx.Channel
{
    public class ChannelPipeline : IChannelPipeline
    {
        readonly ILifetimeScope _scope;
        readonly ILogger<ChannelPipeline> _logger;
        readonly ExceptionMode _exceptionMode;

        public ChannelPipeline(ILogger<ChannelPipeline> logger, ILifetimeScope scope, TestExceptionMode? testExceptionMode = null)
        {
            _scope = scope;
            _logger = logger;
            _exceptionMode = testExceptionMode?.Mode ?? ExceptionMode.Production;
        }

        public async Task<InvocationResponse> ClientPipeLine(InvocationRequest request, IChannel channel, Activity? parentActivity)
        {
            try
            {
                Debug.Assert(channel is not null);
                Type serializerType = channel.Pipeline?.MessageSerializer.ImplementationType ?? typeof(IMessageSerializer);
                Type protectorType = channel.Pipeline?.MessageProtection.ImplementationType ?? typeof(IMessageProtection);
                Parameter[] protectionOptionsParameter = [];
                var protectionOptions = channel.Pipeline?.MessageProtection.Options;
                if (protectionOptions is not null)
                {
                    protectionOptionsParameter = [new TypedParameter(protectionOptions.GetType(), protectionOptions)];
                }
                Abstractions.Protection.ClientContext protectionContext = new ClientContext(){ Contract = channel.Contract, MethodName = request.MethodName};
                var protector =  (IMessageProtection)_scope.Resolve(protectorType, protectionOptionsParameter);
                var serializer = (IMessageSerializer)_scope.Resolve(serializerType);
                byte[] serializedRequest = serializer.Serialize(request, channel.Contract, request.MethodName);
                byte[] protectedRequest = await protector.Protect(serializedRequest, protectionContext).ConfigureAwait(false);
                byte[] protectedResponse = await channel.InvokeResult(protectedRequest).ConfigureAwait(false);
                byte[] serializedResponse = await protector.Unprotect(protectedResponse, protectionContext).ConfigureAwait(false);

                if (serializedResponse.Length == 0)
                {
                    return new InvocationResponse();
                }

                var deserializedResponse = serializer.Deserialize<InvocationResponse>(serializedResponse, channel.Contract, request.MethodName);
                Debug.Assert(deserializedResponse is not null);
                return deserializedResponse;
            }
            catch (ServiceException ex)
            {
                if (_exceptionMode == ExceptionMode.Bare)
                    throw;

                if(_exceptionMode == ExceptionMode.Wrapped)
                    throw new ClientException("Error during service invocation", ex);

                // fall through equivalent to
                // if(_exceptionMode == ExceptionMode.Production)
                throw new ClientException("Error during service invocation");
            }
            catch (Exception ex)
            {
                if (_exceptionMode == ExceptionMode.Bare)
                    throw;

                // fall through equivalent to
                // if(_exceptionOptions.Mode == ExceptionMode.Production || _exceptionOptions.Mode == ExceptionMode.Wrapped)
                throw new ClientException("Error communicationg with service", ex);
            }
        }
    }
}
