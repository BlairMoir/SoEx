# Use State

```
dotnet new webapi -n DaprDemo.D006.UseState --no-https
cd DaprDemo.D006.UseState
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

WeatherForecastController.cs

Using statements
```csharp
using Dapr;
```

Add to GetMethod 
```csharp
[HttpGet("{counter}")]
public IEnumerable<WeatherForecast> Get([FromState("statestore","counter")] StateEntry<int> currentCount)
{
     currentCount.Value++;            
    currentCount.SaveAsync().Wait();
    _logger.LogInformation($"{currentCount.Key}: {currentCount.Value}");
    ...
}
```

run
```
dapr run --app-id DaprDemo-D006-UseState -p 5000 dotnet run
```

open in a browser repeatedly
```
http://localhost:5000/WeatherForecast/london

```

Observe in logs
```
== APP == info: DaprDemo.D006.UseState.Controllers.WeatherForecastController[0]

== APP ==       london: 1

== APP == info: DaprDemo.D006.UseState.Controllers.WeatherForecastController[0]

== APP ==       london: 2
...
```