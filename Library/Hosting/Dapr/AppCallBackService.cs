using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace SoEx.Dapr
{
    public class AppCallBackService : AppCallback.AppCallbackBase
    {
        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ListTopicSubscriptionsResponse());
        }

        public override Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
        {
            return Task.FromResult(new TopicEventResponse());
        }
    }
}
