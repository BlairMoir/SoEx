# Read Secrets


```
dotnet new webapi -n DaprDemo.D005.ReadSecrets --no-https
cd DaprDemo.D005.ReadSecrets
```

Add Dapr library

```
dotnet add package Dapr.AspNetCore 
dotnet add package Dapr.Extensions.Configuration
```


Program.cs 

```csharp
using Dapr.Client;
using Dapr.Extensions.Configuration;
```

add to CreateHostbuilder

```csharp
...
.ConfigureAppConfiguration(config =>
{
    var daprClient = new DaprClientBuilder().Build();
    config.AddDaprSecretStore("my-secret-store", daprClient);
})
...
```

Add to weatherforecastcontroller.cs

```csharp
using Microsoft.Extensions.Configuration;
```

```csharp

...
 private readonly IConfiguration _config;
...

public WeatherForecastController(..., IConfiguration config)
{
    ...
_config = config;
 var secretValue = _config["mysecretkey"];
_logger.LogInformation($"my secret:  {secretValue}");
    ...
}


```

Create Directory components and file
./components/secretStore.yaml

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

Add a .gitignore 
```
*secrets.json
```

Create .components/mysecrets-secrets.json
```json
{
  "mysecretkey": "mysecretvalue"
}
```

run
```
dapr run --app-id DaprDemo-D005-ReadSecrets -p 5000 --components-path ./components  -- dotnet run
```