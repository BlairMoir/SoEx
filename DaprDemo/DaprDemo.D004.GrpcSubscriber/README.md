
Create Project
```
dotnet new grpc -n DaprDemo.D004.GrpcSubscriber
cd DaprDemo.D004.GrpcSubscriber
```

Add Dapr library

```
dotnet add package Dapr.AspNetCore 
```

Update startup.cs

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddDaprClient();
}
```

Update startup.cs

Modify GreeterService.cs 

Add using statements
```csharp
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
```

Changing the base class as below

```csharp
public class GreeterService :  AppCallback.AppCallbackBase
```

remove override from Say Hello Method and add log message
```csharp
public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
{
     var message = "Hello " + request.Name;
    _logger.LogInformation(message);
    return Task.FromResult(new HelloReply
    {
        Message = message
    });
}
```
implement oninvoke to keep dapr happy
```csharp
public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
{
    return Task.FromResult(new InvokeResponse());
}
```

Implement ListTopicSubscriptions 
```csharp
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
```

Implement OnTopicEvent
```csharp
 public override async Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
{
    if (request.PubsubName == "pubsub")
    {          
        var input = JsonSerializer.Deserialize<HelloRequest>(request.Data.ToStringUtf8(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await SayHello(input, context);
    }
    return await Task.FromResult(new TopicEventResponse(){ Status = TopicEventResponse.Types.TopicEventResponseStatus.Success});            
}
```

run
```
dapr run --app-id DaprDemo-D004-GrpcSubscriber --app-port 5000 --app-protocol grpc -H 3500 -- dotnet run
```

publish event
```
Invoke-RestMethod -Method Post -ContentType 'application/json' -Body '{"name": "[your name here]"}' -Uri 'http://localhost:3500/v1.0/publish/pubsub/exampletopic'


curl -X POST http://localhost:3500/v1.0/publish/pubsub/exampletopic -H "Content-Type: application/json" -d '{"name": "[your name here]"}'

```