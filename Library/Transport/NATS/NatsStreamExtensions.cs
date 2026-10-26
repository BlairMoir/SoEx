using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace SoEx.Transport.NATS;

public static class NatsStreamExtensions
{
    private const int StreamNameInUse = 10058;

    public static async Task CreateEventStream<I>(this INatsJSContext jetStream)
    {
        try
        {
            var streamConfig = new StreamConfig()
            {
                Name = NatsSubject.For<I>(),
                Subjects = [NatsSubject.For<I>()],
            };
            await jetStream.CreateStreamAsync(streamConfig).ConfigureAwait(false);
        }
        catch (NatsJSApiException ex) when (ex.Error.ErrCode == StreamNameInUse)
        {
            // stream exists with differnt settings, use it anyway
        }
    }
}
