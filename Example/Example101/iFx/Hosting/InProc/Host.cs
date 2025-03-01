using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using SoEx.InProc;
using Microsoft.AspNetCore.Builder;

namespace Example101.iFx.Hosting;
public static class Host
{    
    public static WebApplicationBuilder InProc(string[] args)
    {
        var hostBuilder = WebApplication.CreateBuilder(args);
        string? assemblyName = typeof(Host).Assembly.FullName;
        Debug.Assert(assemblyName is not null);
        string companyNamespace = assemblyName.Split(".")[0];
        var hostAssemblyName = Assembly.GetCallingAssembly().GetName().Name;
        hostBuilder.InProcIfx(FindServiceTypes(companyNamespace));        
        hostBuilder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty(nameof(hostAssemblyName), hostAssemblyName)
                    .WriteTo.Seq("http://localhost:5341")
                    .WriteTo.Console());       

        hostBuilder.Services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                    resource.AddService(hostAssemblyName).Build()
                 )
                // .WithMetrics( metrics => metrics.AddRuntimeInstrumentation())
                .WithTracing(tracing => tracing
                    .AddSource("SoEx.InProc")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
                        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                    })
        );            

        return hostBuilder;
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
