using System.Diagnostics;
using System.Reflection;
using Example002.Common.Policy;
using Example002.iFx.Contract;
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

namespace Example002.iFx.Hosting;

public static class Host
{
    public static WebApplication Dapr(int port, string[] args)
    {
        string? assemblyName = typeof(Host).Assembly.FullName;
        Debug.Assert(assemblyName is not null);
        string companyNamespace = assemblyName.Split(".")[0];
        var hostAssemblyName = Assembly.GetCallingAssembly().GetName().Name;

        var builder = WebApplication.CreateBuilder(args);
        builder.DaprIfx(AppIdConvention, FindClientInterfaces(companyNamespace));
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
        app.DaprService(FindServiceTypes(companyNamespace));
        return app;
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
            var types = assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IService)));
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
            var serviceInterfaces = assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IService)));
            clientTypes.AddRange(serviceInterfaces);
        }
        return clientTypes.ToArray();
    }

    private static Func<Type, string> AppIdConvention => type => type.Namespace!.Replace(".", "-").Replace("Interface", "Service"); 
}
