using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace SoEx.Dapr
{
    public class AppCallBackService : AppCallback.AppCallbackBase
    {
        private static readonly List<System.Type> _subscriptions = [];
        private static readonly List<Func<string,Task>> _callbacks = [];

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {
            var topicResponse = new ListTopicSubscriptionsResponse();   
            foreach(var subscription in _subscriptions.Select( s=> s.FullName).Distinct())         
            {                
                topicResponse.Subscriptions.Add(new TopicSubscription(){ PubsubName = "pubsub", Topic = subscription});
            }            
            return Task.FromResult(topicResponse);
        }

        public override async Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
        {
            List<Task> callbackTasks = [];
            foreach(var callback in _callbacks)
            {
                var data = request.Data.ToStringUtf8();
                callbackTasks.Add(callback.Invoke(data));
            }
            await Task.WhenAll(callbackTasks);
            return new TopicEventResponse();
        }

        public static void AddSubscriptions(System.Type[] types)
        {
            _subscriptions.AddRange(types);
        }

        public static void RegisterCallBack(Func<string,Task> callback)
        {
            _callbacks.Add(callback);
        }
    }
}
