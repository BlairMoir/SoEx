# Hello WebApi


Create Project
```
dotnet new webapi -n DaprDemo.D001.HelloWebApi
cd DaprDemo.D001.HelloWebApi
```

Add Dapr library

```
dotnet add package Dapr.AspNetCore 
```

Update startup.cs

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers().AddDapr();                       
}
```

run
```
dapr run --app-id DaprDemo-D001-HelloWebApi -p 5000 -H 5020 dotnet run
```

open in a browser
```
http://localhost:5020/v1.0/invoke/DaprDemo-D001-HelloWebApi/method/WeatherForecast

```