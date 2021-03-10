# WebApi Subscriber


Create Project
```
dotnet new webapi -n DaprDemo.D003.WebApiSubscriber --no-https
cd DaprDemo.D003.WebApiSubscriber
```

Add Dapr library

```
dotnet add package Dapr.AspNetCore 
```

Update startup.cs

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers().AddDapr();                       
}
```

```csharp
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseCloudEvents();
    ...
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapSubscribeHandler();
        ...
    });
}
```
Using statements
```csharp
using Dapr;
```

Replace the Get() Method in Weather ForecastController with

```csharp
[Topic("pubsub", "exampletopic")]
public WeatherForecast Get([FromBody]WeatherForecast value)
{
    _logger.LogInformation("Subscription Invoked");
    _logger.LogInformation(value.Summary);   
    return value;
}
```

run
```
dapr run --app-id DaprDemo-D003-WebApiSubscriber -p 5000 -H 3500 dotnet run
```

publish event
```
Invoke-RestMethod -Method Post -ContentType 'application/json' -Body '{"Summary": "test"}' -Uri 'http://localhost:3500/v1.0/publish/pubsub/exampletopic'


curl -X POST http://localhost:3500/v1.0/publish/pubsub/exampletopic -H "Content-Type: application/json" -d '{"Summary": "test"}'

```