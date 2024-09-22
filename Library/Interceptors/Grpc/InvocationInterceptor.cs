using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace SoEx.Grpc
{
    public class InvocationInterceptor : Interceptor
    {
        ILogger<InvocationInterceptor> _logger;
        public InvocationInterceptor(ILogger<InvocationInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string[] parts = context.Method.Split("/");
            string operation = parts.Last();
            string @namespace = parts[parts.Length - 2];
            parts = @namespace.Split(".");
            string service = parts.Last();

            _logger.LogInformation("{Service} {Operation} Started", service, operation);
            TResponse result = await continuation(request, context);
            _logger.LogInformation("{Service} {Operation} Ended", service, operation);
            return result;
        }
    }
}
