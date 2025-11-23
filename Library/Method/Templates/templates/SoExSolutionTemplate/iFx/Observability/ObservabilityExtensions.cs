using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace SoExTemplate.iFx.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection ConfigureTelemetry(this IServiceCollection services)
    {
        string hostAssemblyName = EntryAssemblyHostName();
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

    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        string hostAssemblyName = EntryAssemblyHostName();
        services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(nameof(hostAssemblyName), hostAssemblyName)
            .WriteTo.Seq("http://localhost:5341")
            .WriteTo.Console());
        return services;
    }

    private static string EntryAssemblyHostName()
    {
        return Assembly.GetEntryAssembly()?.GetName().Name ??  "unknown host";
    }
}
