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

    public static async Task<IConnection> ConnectAsync<I>(this RabbitEventBinding<I> binding, string connectionName)
    {
        var addresses = binding.Transport.Address.Uris;
        Uri[] uris = [.. addresses.Select( (address,node) => WithCredentials(address, binding.RabbitConfig[node])).ToArray()];

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

    private static Uri WithCredentials(Uri address, RabbitConfig rabbitConfig)
    {
        var uriBuilder = new UriBuilder(address);
        uriBuilder.UserName = Uri.EscapeDataString(rabbitConfig?.UserName ?? "guest");
        uriBuilder.Password = Uri.EscapeDataString(rabbitConfig?.Password ?? "guest");
        return uriBuilder.Uri;
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
