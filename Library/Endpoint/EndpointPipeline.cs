using System.Diagnostics;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Abstractions.Protection;
using SoEx.Exceptions;
using SoEx.Topology;

namespace SoEx.Endpoint
{
    public class EndpointPipeline : IEndpointPipeline
    {
        readonly IHostAndClientLookup _subsystemlifeTimeScope;
        readonly ILogger<EndpointPipeline> _logger;
        readonly ExceptionMode _exceptionMode;
        readonly ITelemetryConfidentiality _telemetryConfidentiality;

        public EndpointPipeline(ILogger<EndpointPipeline> logger, IHostAndClientLookup subsystemlifeTimeScope, ITelemetryConfidentiality  telemetryConfidentiality, TestExceptionMode? testExceptionMode = null)
        {
            _subsystemlifeTimeScope = subsystemlifeTimeScope;
            _logger = logger;
            _exceptionMode = testExceptionMode?.Mode ?? ExceptionMode.Production;
            _telemetryConfidentiality = telemetryConfidentiality;
        }

        public async Task<byte[]> ServicePipeLine<I>(byte[] payload, IBindingPipeline? pipeline, Activity? parentActivity) where I : class
        {
            try
            {
                Type dispatcherType = pipeline?.Dispatcher.ImplementationType ?? typeof(IDispatcher);
                Type serializerType = pipeline?.MessageSerializer.ImplementationType ?? typeof(IMessageSerializer);
                Type protectorType = pipeline?.MessageProtection.ImplementationType ?? typeof(IMessageProtection);

                Parameter[] protectionOptionsParameter = [];
                var protectionOptions = pipeline?.MessageProtection.Options;
                if (protectionOptions is not null)
                {
                    protectionOptionsParameter = [new TypedParameter(protectionOptions.GetType(), protectionOptions)];
                }

                Abstractions.Protection.MessageContext
                    protectionContext = new MessageContext() { Contract = typeof(I) };
                var subSystemHost = _subsystemlifeTimeScope.For<ISubSystemHost<I>>();
                using (Autofac.ILifetimeScope requestScope = subSystemHost.BeginRequestLifetimeScope())
                {
                    var dispatcher = (IDispatcher)requestScope.Resolve(dispatcherType);
                    var serializer = (IMessageSerializer)requestScope.Resolve(serializerType);
                    var protector = (IMessageProtection)requestScope.Resolve(protectorType,protectionOptionsParameter);

                    byte[] serializedRequest = await protector.Unprotect(payload, protectionContext);
                    InvocationRequest? request = serializer.Deserialize<InvocationRequest>(serializedRequest, typeof(I));
                    using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(EndpointPipeline)}", ActivityKind.Server, request?.ActivityId))
                    {
                        Debug.Assert(request is not null);
                        var response = await dispatcher.Dispatch<I>(request);
                        byte[] serializedResponse = serializer.Serialize(response, typeof(I), request.MethodName);
                        byte[] protectedResponse = await protector.Protect(serializedResponse, protectionContext);
                        return protectedResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error executing service pipeline: {ExceptionType}, {ExceptionStackTrace}", ex.GetType(), ex.StackTrace);
                _logger.LogDebug("{ExceptionMessage}", _telemetryConfidentiality.Protect(ex.Message));
                if (_exceptionMode == ExceptionMode.Bare)
                    throw;

                throw new ServiceException("Error executing service pipeline", ex);
            }
        }
    }
}
