using System.Diagnostics;
using System.Reflection;
using Example003.Common.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SoEx.Context;
using SoEx.Dapr;
using SoEx.PubSub;
using SoEx.PubSub.Dapr;

namespace Example003.iFx.Hosting;

public static class Host
{
    private static string[] _serviceSuffixConventionKeywords = ["Manager", "Engine", "Access", "Utility"];


    public static WebApplication Dapr(int port, string[] args)
    {
        string? assemblyName = typeof(Host).Assembly.FullName;
        Debug.Assert(assemblyName is not null);
        string companyNamespace = assemblyName.Split(".")[0];
        var hostAssemblyName = Assembly.GetCallingAssembly().GetName().Name;

        var builder = WebApplication.CreateBuilder(args);
        Type[] serviceTypes = builder.ScanAssembliesAndUseDaprIfx(companyNamespace);
        builder.Services.ConfigureLogging();
        builder.Services.ConfigureTelemetry();
        builder.Services.AddSingleton<IContextFlowPolicy, ContextFlowPolicy>();
        builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(port, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            });
        var app = builder.Build();
        app.DaprService(serviceTypes);
        return app;
    }

    private static Type[] ScanAssembliesAndUseDaprIfx(this WebApplicationBuilder builder, string companyNamespace) {
            Type[] subscriptionInterfaceTypes = FindSubscriptionInterfaceTypes(companyNamespace);
            Type[] serviceTypes = FindServiceTypes(companyNamespace);
            Type[] clientInterfaceTypes = FindClientInterfaces(companyNamespace);
            EventListener[] eventListeners = MapEventListeners(subscriptionInterfaceTypes, serviceTypes);
            
            builder.DaprIfx(AppIdConvention, clientInterfaceTypes, eventListeners);
            builder.WithPubSub();
            builder.DaprSubscriptions(subscriptionInterfaceTypes);
            AppCallBackService.RegisterCallBack(SubscribeListener.Process);
            
            return serviceTypes;
        }

    private static EventListener[] MapEventListeners(Type[] subscriptionInterfaceTypes, Type[] serviceTypes)
    {
        List<EventListener> eventListeners = new List<EventListener>();
        foreach (var serviceType in serviceTypes)
        {
            var serviceInterfaces = serviceType.GetInterfaces();
            foreach (var serviceInterface in serviceInterfaces)
            {
                var listnerInterface = subscriptionInterfaceTypes.SingleOrDefault(w => w.FullName == serviceInterface.FullName);
                if (listnerInterface is not null)
                {
                    eventListeners.Add(new EventListener() { EventService = serviceType, EventInterface = listnerInterface });
                }
            }
        }
        return eventListeners.ToArray();
    }

    private static Type[] FindSubscriptionInterfaceTypes(string company)
    {
        List<Type> serviceTypes = [];
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Interface.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => t.Name.EndsWith("Events"));
            serviceTypes.AddRange(types);
        }
        return serviceTypes.ToArray();
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
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddZipkinExporter(c => c.Endpoint = new Uri("http://localhost:9411/api/v2/spans"))
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
                        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                    })
        );
        return services;
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

    private static Type[] FindServiceTypes(string company)
    {
        List<Type> serviceTypes = [];
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Service.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => _serviceSuffixConventionKeywords.Any(s => t.Name.EndsWith(s)));
            serviceTypes.AddRange(types);
        }

        return serviceTypes.ToArray();
    }

    private static Type[] FindClientInterfaces(string company)
    {
        List<Type> clientTypes = [];
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.Interface.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var serviceInterfaces = assembly.GetTypes().Where(t => _serviceSuffixConventionKeywords.Any(s => t.Name.EndsWith(s)));
            clientTypes.AddRange(serviceInterfaces);
        }
        return clientTypes.ToArray();
    }
    private static Func<Type, string> AppIdConvention => type => type.Namespace!.Replace(".", "-").Replace("Interface", "Service"); 

}
