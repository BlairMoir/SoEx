using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;

namespace SoEx.Transport.RabbitMq;

public static class RabbitExtensions
{
    public static IServiceCollection RabbitClient(this IServiceCollection collection)
    {
        collection.AddTransient(typeof(RabbitEventChannel<>), typeof(RabbitEventChannel<>));
        return collection;
    }

    public static async Task<IConnection> ConnectAsync(this RabbitConfig? config, string connectionName)
    {
        var hostnames = config?.HostName?.Split(',') ?? [];
        if (hostnames.Length <= 1)
        {
            var builder = Credentials(ConnectionSettingsBuilder.Create().ContainerId(connectionName), config);
            if (hostnames.Length == 1)
            {
                builder.Host(hostnames[0]);
            }
            var connectionSettings = builder.Build();
            return await AmqpConnection.CreateAsync(connectionSettings);
        }

        Uri[] uris = [.. hostnames.Select(host => NodeUri(host, config))];
        Exception? lastFailure = null;
        for (int first = 0; first < uris.Length; first++)
        {
            var settingsBuilder = ConnectionSettingsBuilder.Create().ContainerId(connectionName)
                .Uris(uris).UriSelector(new RotatingUriSlector(first));
            var settings = settingsBuilder.Build();
            try
            {
                return await AmqpConnection.CreateAsync(settings);
            }
            catch (Exception ex)
            {
                lastFailure = ex;
            }
        }
        throw lastFailure!;
    }

    private static Uri NodeUri(string hostname, RabbitConfig? config)
    {
        var uriBuilder = new UriBuilder("amqp", hostname, 5672);
        uriBuilder.UserName =  Uri.EscapeDataString(config?.UserName ?? "guest");
        uriBuilder.Password =  Uri.EscapeDataString(config?.Password ?? "guest");
        var virtualHost = Uri.EscapeDataString(config?.VirtualHost ?? "/");
        uriBuilder.Path = $"/{virtualHost}";
        var uri = uriBuilder.Uri;
        return uri;
    }

    private static ConnectionSettingsBuilder Credentials(ConnectionSettingsBuilder builder, RabbitConfig? config)
    {
        if (config?.UserName is not null)
        {
            builder.User(config.UserName);
        }
        if (config?.Password is not null)
        {
            builder.Password(config.Password);
        }
        if (config?.VirtualHost is not null)
        {
            builder.VirtualHost(config.VirtualHost);
        }
        return builder;
    }

    private sealed class RotatingUriSlector(int first) : IUriSelector
    {
        private int _next = first;
        public Uri Select(ICollection<Uri> uris)
        {
            var index = (Interlocked.Increment(ref _next) - 1) % uris.Count;
            return uris.ElementAt(index);
        }
    }
}
