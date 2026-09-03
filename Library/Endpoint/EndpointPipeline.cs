using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
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

        public async Task<byte[]> ServicePipeLine<I>(byte[] payload, IPipeline? pipeline, Activity? parentActivity) where I : class
        {
            try
            {
                Type dispatcherType = pipeline?.Dispatcher ?? typeof(IDispatcher);
                Type serializerType = pipeline?.MessageSerializer ?? typeof(IMessageSerializer);
                Type protectorType = pipeline?.MessageProtection ?? typeof(IMessageProtection);
                var subSystemHost = _subsystemlifeTimeScope.For<ISubSystemHost<I>>();
                using (Autofac.ILifetimeScope requestScope = subSystemHost.BeginRequestLifetimeScope())
                {
                    var dispatcher = (IDispatcher)requestScope.Resolve(dispatcherType);
                    var serializer = (IMessageSerializer)requestScope.Resolve(serializerType);
                    var protector = (IMessageProtection)requestScope.Resolve(protectorType);

                    byte[] serializedRequest = await protector.Unprotect(payload);
                    InvocationRequest? request = serializer.Deserialize<InvocationRequest>(serializedRequest, typeof(I));
                    using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(EndpointPipeline)}", ActivityKind.Server, request?.ActivityId))
                    {
                        Debug.Assert(request is not null);
                        var response = await dispatcher.Dispatch<I>(request);
                        byte[] serializedResponse = serializer.Serialize(response, typeof(I), request.MethodName);
                        byte[] protectedResponse = await protector.Protect(serializedResponse);
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
