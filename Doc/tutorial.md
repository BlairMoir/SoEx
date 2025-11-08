---
outline: [2,4]
---
# Tutorial

[[toc]]


::: info
For simplicity we are going to use some helpers that follow the conventions of the IDesign Method, you can learn more about
that by reading [Righting Software](https://www.amazon.com/dp/0136524036). For an example hosting a
service with your own naming see
[HowTo: Host your first inproc service](http://localhost:5173/howto.html#host-your-first-inproc-service)
:::

## Create a service oriented project and host it with SoEx

::: tip
This requires the method.solution and method.component templates. [HowTo: Setup Templates](howto/#setup-templates)
:::

 ### Create a solution

The first step is to create a solution, this template includes a default Host and default glue to
connect everything together. The glue layers also called "iFx" is where you can update your own choices
of telemetry, logging, or conventions without having to alter either SoEx or your business code.

 ```bash
 dotnet new method.solution -n Example123 -o Example123
 ```

### Create manager interface and service projects

```bash
cd Example123
dotnet new method.component -n Example123 --componentType Manager --componentName Membership
```
Add them to the solution

```bash
dotnet sln add Component/*/*/Interface/
dotnet sln add Component/*/*/Service/
```

Reference the service from the Host

```bash
dotnet reference add Component/*/*/Service/ --project Host/InProc/Example123.Host.InProc.csproj
```

In the directory: `Component/Manager/Membership/Interface`. Create the interface file: `IMembershipManager.cs`

```c#
namespace Example123.Manager.Membership.Interface
{
    public interface IMembershipManager
    {
        Task Profile();
    }
}
```

::: tip
If this part feels a bit manual don't worry. When working on a real system and not following a tutorial, you can automate
the creation of Interfaces and the referencing of projects by using [MethodSketch](https://methodsketch.app)
:::

In the directory: `Component/Manager/Membership/Service`. create the implementation for the service inside file: `MembershipManager.cs`

```c#
namespace Example123.Manager.Membership.Service
{
    public class MembershipManager : IMembershipManager
    {
        public Task Profile()
        {
            return Task.CompletedTask;
        }
    }
}
```

### Call the Manager service
In the file: Example123/Host/InProc/Program.cs

Change
```c#
// var proxy = Proxy.ForService<I_XXX_Manager>();
// await proxy._Operation_();
throw new NotImplementedException();
```

to

```c#
var proxy = Proxy.ForService<IMembershipManager>();
await proxy.Profile();
```

Set a breakpoint inside the manager implementation and hit debug!

### Create an access

Next we are going to create a UserAccess for the manger to retrieve the profile

```bash
dotnet new method.component -n Example123 --componentType Access --componentName User
```
Add them to the solution

```bash
dotnet sln add Component/*/*/Interface/
dotnet sln add Component/*/*/Service/
```

Reference the service from the Host

```bash
dotnet reference add Component/*/*/Service/ --project Host/InProc/Example123.Host.InProc.csproj
```

Reference the access interface from the manager
```bash
dotnet reference add Component/Access/User/Interface/ --project Component/Manager/Membership/Service/Example123.Manager.Membership.Service.csproj
```

In the directory: `Component/Access/User/Interface`. Create the interface file: `IUserAccess.cs`

```c#
namespace Example123.Access.User.Interface
{
    public interface IUserAccess
    {
        Task Load();
    }
}
```

In the directory: `Component/User/Access/Service`. create the implementation for the service inside file: `UserAccess.cs`

```c#
namespace Example123.Access.User.Service
{
    public class UserAccess : IUserAccess
    {
        public Task Load()
        {
            return Task.CompletedTask;
        }
    }
}
```

### Call the access from the manager

#### Using DI

We will start with obtaining the proxy by dependency injection as that is what most people are familiar with.

Update `MembershipManager.cs`

```c#
public class MembershipManager(IUserAccess userProxy) : IMembershipManager
{
    public async Task Profile()
    {
        await userProxy.Load();
    }
}
```

set a breakpoint in the UserAccess and hit debug!


#### Proxy Helper

Instead of littering the constructor that is used by every single operation, we can use the proxy helper.

Update `MembershipManager.cs`

```c#
public class MembershipManager : IMembershipManager
{
    public async Task Profile()
    {
        var userProxy = Proxy.ForComponent<IUserAccess>(this);
        await userProxy.Load();
    }
}
```

Hit debug and check all your breakpoints are hit.


It is likely that part of the audience is new to
[service-orientation](explanation#service-disambiguation) and is screaming at me that this is a service locator and the
[Service Locator is an anti-pattern](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/).
All the examples of this being an anti-pattern are connected to [object-orientation](explanation#service-disambiguation).

First we are requesting a Proxy to the service and not the service itself. Second we do not know if the service we
are connecting to is:
* in process
* out of process
* over a network
* on the same machine,
* is a single instance or a cluster of multiple instances

The proxy is going to take an address and using some mechanism of discovery locate the endpoint.
Then use the endpoint information to open a channel to communicate with the service.

Regardless of if you acquire your proxy from a static helper or the constructor, the same registration is invoked

```c#
builder.Register(c => c.Resolve<ProxyFactory>().Create(client.Service.Contract)).As(client.Service.Contract);
```

The ProxyFactory returns you a proxy that has no target behind it, instead the invocation is going to he handled
by an interceptor

```c#
public object Create(Type serviceInterface)
{
    return s_proxyGenerator.CreateInterfaceProxyWithoutTarget(serviceInterface, _proxyInterceptor);
}
```

The interceptor does alot of work beyond the scope of this tutorial: Retrieving the client to get the binding,
creating the channel and following the ChannelPipeline


::: info
Every invocation in SoEx is per call so anything added for another operation will be built and discard on
every single call. Dependency injection is best reserved for the object-oriented implementation within your service.
When communicating between services it is your choice, but the author of the framework thinks its cleaner and
more understandable to just use the helper and not be dogmatic.
:::

## Logging and tracing

Now lets look at the built in logs and traces. You can either install [Seq](https://datalust.co/) or update
methods `ConfigureTelemetry()` and `ConfigureLogging()` in file `/iFx/Hosting/InProc/Host.cs` to use your favourite tool.

Once you have logging setup lets run the project again. Start up and first run is always a bit slower, to keep
image a bit smaller this screenshot is from a second request.

![InProc Trace](images/TutorialInProcTrace.png)

## Ambient Context

## Swapping to another binding

Lets explore creating another set of hosts alongside!

```bash
dotnet new classlib -n Example123.iFx.Hosting.Component -o iFx/Hosting/Component
dotnet sln add iFx/Hosting/Component/

dotnet new console --use-program-main -n Membership.Example123.Manager.Membership.Host -o Host/PerComponent/Membership/Membership/
dotnet new console --use-program-main -n Membership.Example123.Access.User.Host -o Host/PerComponent/Membership/User/
dotnet new console --use-program-main -n Example123.PerComponent.Client -o Host/PerComponent/Client/
dotnet sln add Host/PerComponent/Membership/Membership/
dotnet sln add Host/PerComponent/Membership/User/
dotnet sln add Host/PerComponent/Client/


dotnet reference add Component/Manager/Membership/Service/ --project Host/PerComponent/Membership/Membership/Membership.Example123.Manager.Membership.Host.csproj
dotnet reference add iFx/Hosting/Component --project Host/PerComponent/Membership/Membership/Membership.Example123.Manager.Membership.Host.csproj
dotnet reference add Component/Access/User/Service/ --project Host/PerComponent/Membership/User/Membership.Example123.Access.User.Host.csproj
dotnet reference add iFx/Hosting/Component --project Host/PerComponent/Membership/User/Membership.Example123.Access.User.Host.csproj
dotnet reference add Component/Manager/Membership/Interface/ --project Host/PerComponent/Client/Example123.PerComponent.Client.csproj

dotnet package add SoEx.Topology --project Host/PerComponent/Client/Example123.PerComponent.Client.csproj --prerelease
 dotnet package add SoEx.Context.Abstractions --project Host/PerComponent/Client/Example123.PerComponent.Client.csproj --prerelease
dotnet package add Microsoft.Extensions.Hosting --project Host/PerComponent/Client/Example123.PerComponent.Client.csproj --prerelease
dotnet package add SoEx.Hosting --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj --prerelease
dotnet package add SoEx.Transport.NamedPipe --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj --prerelease
dotnet package add OpenTelemetry --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj
dotnet package add OpenTelemetry.Exporter.OpenTelemetryProtocol --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj
dotnet package add OpenTelemetry.Extensions.Hosting --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj
dotnet package add Serilog.Extensions.Hosting --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj
dotnet package add Serilog.Sinks.Console --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj
dotnet package add Serilog.Sinks.Seq --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj
dotnet package add Serilog.Sinks.Seq --project iFx/Hosting/Component/Example123.iFx.Hosting.Component.csproj
```

Create the file `Host.cs` in `iFx/Hosting/Component`

```c#
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SoEx.Context;
using SoEx.Topology;
using SoEx.Transport.NamedPipe;

namespace Example123.iFx.Hosting.Component;

public static class Host
{
    public static IHost NamedPipe(string[] args, ServiceCollection? sc = null)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.FullName;
        var namespaceParts = assemblyName!.Split(".");
        string subSystem = namespaceParts[0];
        string companyName = namespaceParts[1];

        var services = FindServiceTypes(companyName);
        var interfaces = FindProxyTypes(companyName);
        var contextPolicies = ContextPolicyTypes(companyName);

        if (services.Length == 0)
            throw new ArgumentException("You must include a reference to at least one service");

        if (services.Length > 1)
            throw new ArgumentException("You can only reference on service");

        var applicationBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

        var theService = services[0];
        var serviceFacets = theService.GetInterfaces();
        IEnumerable<Type> proxies = FilterProxies(interfaces, serviceFacets, theService);

        List<Binding> bindings = new List<Binding>();
        foreach (var facet in serviceFacets.Where(w => !(w.FullName?.EndsWith("Event") ?? false)))
        {
            Binding binding = CreateNamedPipeBinding(subSystem, facet);
            bindings.Add(binding);
        }

        List<Client> clients = new List<Client>();
        foreach (var proxy in proxies)
        {
            var genericType = typeof(Client<>).MakeGenericType(proxy);
            var client = (Client)Activator.CreateInstance(genericType)!;
            typeof(Client).GetProperty(nameof(Client.SubSystem))!.SetValue(client, subSystem);
            typeof(Client).GetProperty(nameof(Client.Service))!.SetValue(client, CreateNamedPipeBinding(subSystem, proxy));
            clients.Add(client);
        }

        SoEx.Topology.Host host = new SoEx.Topology.Host()
        {
            Implementation = theService,
            Endpoints = [.. bindings],
            Proxies = [.. clients],
            ServiceCollection = sc
        };
        SoEx.Hosting.HostExtensions.SoEx(applicationBuilder, host);

        foreach (var policy in contextPolicies)
        {
            applicationBuilder.Services.AddSingleton(typeof(IContextFlowPolicy), policy);
        }
        applicationBuilder.Services.NamedPipedClient();
        applicationBuilder.Services.ConfigureLogging();
        applicationBuilder.Services.ConfigureTelemetry();

        var applicationHost = applicationBuilder.Build();
        return applicationHost;
    }

    private static IEnumerable<Type> FilterProxies(Type[] interfaces, Type[] serviceFacets, Type theService)
    {
        var proxies = interfaces
            .Except(serviceFacets)
            .Where(w => !w.Name.EndsWith("Manager"));
        return proxies;
    }

    private static Binding CreateNamedPipeBinding(string subSystem, Type facet)
    {
        var genericType = typeof(NamedPipeBinding<>).MakeGenericType(facet);
        var binding = (Binding)Activator.CreateInstance(genericType, subSystem)!;
        return binding;
    }

    private static Type[] FindServiceTypes(string company)
    {
        string[] serviceSuffixConventionKeywords = ["Manager", "Engine", "Access", "Utility"];
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Service.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => serviceSuffixConventionKeywords.Any(s => t.Name.EndsWith(s)));
            foundTypes.AddRange(types);

        }
        return foundTypes.ToArray();
    }

    private static Type[] FindProxyTypes(string company)
    {
        string[] serviceSuffixConventionKeywords = ["Manager", "Engine", "Access", "Utility", "Event"];
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Interface.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => serviceSuffixConventionKeywords.Any(s => t.Name.EndsWith(s) && t.IsInterface));
            foundTypes.AddRange(types);
        }
        return foundTypes.ToArray();
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        string? hostAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty(nameof(hostAssemblyName), hostAssemblyName)
                .WriteTo.Seq("http://localhost:5341")
                .WriteTo.Console());
        return services;
    }

    public static IServiceCollection ConfigureTelemetry(this IServiceCollection services)
    {
        string? hostAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        Debug.Assert(hostAssemblyName is not null);
        services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                    resource.AddService(hostAssemblyName).Build()
                )
                .WithTracing(tracing => tracing
                    .AddSource(SoEx.Diagnostics.ActivitySourceNames.Client)
                    .AddSource(SoEx.Diagnostics.ActivitySourceNames.Host)
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri($"http://localhost:5341/ingest/otlp/v1/traces");
                        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                    })
        );
        return services;
    }

    private static Type[] ContextPolicyTypes(string company)
    {
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Policy.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IContextFlowPolicy)));
            foundTypes.AddRange(types);
        }
        return foundTypes.ToArray();
    }
}

```

Update both Component hosts `Program.cs` to be.

```c#
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Example123.iFx.Hosting.Component.Host.NamedPipe(args);
        await builder.RunAsync();
    }
}
```


...

Do some work here to add some stuff to the iFx instead of being duplicated will simplify both the hosts file above and
the creation of the client.
* TestClient
* Logging

the program content below assumes this has been done (sorry if your following before I get to it!
Everything you need already exists somewhere in the solution )

...

Update the component client `Program.cs`

```c#
    static async Task Main(string[] args)
    {
        SoEx.Topology.System ClientSystemTopology = new SoEx.Topology.System()
        {
            Clients = [
                new SoEx.Topology.Client<IMembershipManager>() { Service = new NamedPipeBinding<IMembershipManager>("Membership"), SubSystem = "Membership" },
            ],
            SubSystems = []
        };

        var hostBuilder = Host.CreateApplicationBuilder(args);
        ConfigureLogging(hostBuilder.Services);
         ConfigureTelemetry(hostBuilder.Services);
        hostBuilder.Services.NamedPipedClient();
        hostBuilder.Services.AddTransient<IContextFlowPolicy, Common.Policy.ContextFlowPolicy>();
        hostBuilder.SoEx(ClientSystemTopology);
        hostBuilder.AddTestClient(TestClient, shutdownWhenCompleted: true);

        await hostBuilder.Build().RunAsync();
    }

    static async Task TestClient(ILifetimeScope lifetimeScope)
    {
        System.Diagnostics.Debugger.Break();
        using (var requestScope = lifetimeScope.BeginLifetimeScopeAsyncLocal())
        {
            IMembershipManager membershipManager = Proxy.ForService<IMembershipManager>();
            await membershipManager.Profile();
        }
        await Task.Delay(1000);
    }
```

Then configure all the projects within PerComponent to run together and start debugging.
