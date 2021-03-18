using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json;

namespace DaprDemo.D004.GrpcSubscriber
{
    public class GreeterService :  AppCallback.AppCallbackBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {                  
            return Task.FromResult(new InvokeResponse());
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {    
            var result = new ListTopicSubscriptionsResponse();        
            result.Subscriptions.Add(new TopicSubscription()
            {
                PubsubName = "pubsub",
                Topic = "exampletopic"
            });
            return Task.FromResult(result);
        }
        public override async Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
        {
            if (request.PubsubName == "pubsub")
            {          
                var input = JsonSerializer.Deserialize<HelloRequest>(request.Data.ToStringUtf8(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await SayHello(input, context);
            }
            return await Task.FromResult(new TopicEventResponse(){ Status = TopicEventResponse.Types.TopicEventResponseStatus.Success});            
        }

        public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var message = "Hello " + request.Name;
            _logger.LogInformation(message);
            return Task.FromResult(new HelloReply
            {
                Message = message
            });
        }
    }
}
