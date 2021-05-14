using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using System.Text;

namespace DaprDemo.D007.InputBinding
{
    public class GreeterService : AppCallback.AppCallbackBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<ListInputBindingsResponse> ListInputBindings(Empty request, ServerCallContext context)
        {
            var result = new ListInputBindingsResponse();   
            result.Bindings.Add("my-storage-binding");
            return Task.FromResult(result);
        }

        public override Task<BindingEventResponse> OnBindingEvent(BindingEventRequest request, ServerCallContext context)
        {
            //Message on azure storage queue might be utf8 or base64 encoded
            var dataUtf8 = request.Data.ToStringUtf8();
            int byteswritten = 0;
            Span<byte> bytes = new Span<byte>(new byte[(3 * (dataUtf8.Length / 4)) - dataUtf8.Count( w => w == '=')]);
            Convert.TryFromBase64String(dataUtf8, bytes ,out byteswritten);
            var fromBase64 = Encoding.UTF8.GetString(bytes);
            _logger.LogInformation($"Binding {request.Name} Received {dataUtf8} ({fromBase64})") ;
            return Task.FromResult (new BindingEventResponse());
        }

    }
}
