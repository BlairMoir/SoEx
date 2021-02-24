using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;

namespace DaprDemo.D002.HelloGrpc
{
    public class GreeterService : AppCallback.AppCallbackBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;            
        }

        public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            var response = new InvokeResponse();
            switch (request.Method)
            {
                case "sayhello":                
                    var input = request.Data.Unpack<HelloRequest>();
                    var output = await SayHello(input, context);
                    response.Data = Any.Pack(output);
                    break;       
                default:
                    break;
            }
            return response;
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {            
            return Task.FromResult(new ListTopicSubscriptionsResponse());
        }

        public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }        
    }
}
