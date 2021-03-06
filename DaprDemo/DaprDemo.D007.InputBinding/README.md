
Create Project
```
dotnet new grpc -n DaprDemo.D007.InputBinding
cd DaprDemo.D007.InputBinding
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

remove Say Hello Method
```csharp
public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
{
   ...
}
```

Add binding methods

```csharp
public override Task<ListInputBindingsResponse> ListInputBindings(Empty request, ServerCallContext context)
{
    var result = new ListInputBindingsResponse();   
    result.Bindings.Add("my-storage-binding");
    return Task.FromResult(result);
}

public override Task<BindingEventResponse> OnBindingEvent(BindingEventRequest request, ServerCallContext context)
{
    var data = request.Data.ToStringUtf8();
    _logger.LogInformation($"{request.Name} Received {data}") ;
    return Task.FromResult (new BindingEventResponse());
}
```

create azure storeage queue
create components folder
create storagebinding - storageBinding.yaml
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: my-storage-binding
spec:
  type: bindings.azure.storagequeues
  version: v1
  metadata:
  - name: storageAccount
    value: "[your storage account name]"
  - name: storageAccessKey
    secretKeyRef:
      name: access-key
      key: access-key
  - name: queue
    value: "dapr"
  - name: ttlInSeconds
    value: "60"
auth:
  secretStore: my-secret-store
```
setup secrets - secretStore.yaml
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name:  my-secret-store  
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: ./components/mysecrets-secrets.json
  - name: nestedSeparator
    value: ":"
```
mysecrets-secrets.json
```json
{
    "access-key" : "[your access key]" 
}
```

Send message to storage queue. hacky code commited in example to ensure we see some portion of our message correctly using either UTF8 or base64
