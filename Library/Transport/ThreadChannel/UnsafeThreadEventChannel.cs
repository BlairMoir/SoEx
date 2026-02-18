using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;


namespace SoEx.Transport.ThreadChannel
{
    public class UnsafeThreadEventChannel<I> where I : class
    {
        private readonly Channel<byte[]> _channel;

        public UnsafeThreadEventChannel(ILogger<UnsafeThreadEventChannel<I>> logger)
        {
            _channel = System.Threading.Channels.Channel.CreateBounded<byte[]>(1000);
            if (!Debugger.IsAttached)
            {
                logger.LogCritical("Do not use in production! A debugger was not detected, this pubsub will lose messages. Implement a reliable channel for production");
            }
        }

        public ChannelReader<byte[]> Reader => _channel.Reader;
        public ChannelWriter<byte[]> Writer => _channel.Writer;
    }

}
