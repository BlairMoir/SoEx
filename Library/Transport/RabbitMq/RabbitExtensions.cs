using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace SoEx.Transport.RabbitMq;

public static class RabbitExtensions
{
    public static IServiceCollection RabbitClient(this IServiceCollection collection)
    {
        collection.AddSingleton(typeof(RabbitEventChannel<>), typeof(RabbitEventChannel<>));
        return collection;
    }

    public static ConnectionFactory ConfigureFactory(this ConnectionFactory factory, RabbitConfig? config)
    {
        if (config != null)
        {
            if (config.UserName is not null)
            {
                factory.UserName = config.UserName;
            }
            if (config.Password is not null)
            {
                factory.Password = config.Password;
            }
            if (config.VirtualHost is not null)
            {
                factory.VirtualHost = config.VirtualHost;
            }
        }
        return factory;
    }

    public static Task<IConnection> CreateConnectionFromConfig(this ConnectionFactory factory, RabbitConfig? config)
    {
        if (config?.HostName is not null)
        {
            var hostnames = config.HostName.Split(',');
            return factory.CreateConnectionAsync(hostnames);
        }
        return factory.CreateConnectionAsync();
    }
}
