
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SoEx.InProc;
using SoEx.PubSub;

namespace Example003.iFx.Hosting;
public static class Host
{
    public static IHostBuilder InProc(string[] args)
    {
        string? assemblyName = typeof(Host).Assembly.FullName;
        Debug.Assert(assemblyName is not null);
        string companyNamespace = assemblyName.Split(".")[0];

        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .InProcIfx(FindServiceTypes(companyNamespace))
                .WithPubSub()
                .ConfigureServices(services => services
                    .ConfigureLogging()
                    .ConfigureTelemetry()
                );
    }

    public static IServiceCollection ConfigureTelemetry(this IServiceCollection services)
    {
        string? hostAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        Debug.Assert(hostAssemblyName is not null);
        services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                    resource.AddService(hostAssemblyName).Build()
                 )
                // .WithMetrics( metrics => metrics.AddRuntimeInstrumentation())
                .WithTracing(tracing => tracing
                    .AddSource("SoEx.InProc")
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
}
