using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace SoEx.PubSub.InProc
{
    public class PubSubChannel
    {
        private readonly Channel<PubSubEventMessage> _channel;

        public PubSubChannel(ILogger<PubSubChannel> logger){
            _channel = Channel.CreateBounded<PubSubEventMessage>(1000);
            if(!Debugger.IsAttached)
            {
                logger.LogCritical("Do not use in production! A debugger was not detected, this pubsub will lose messages. Implement a reliable channel for production");
                throw new NotSupportedException("Implement a reliable channel PubSubChannel for production");
            }
        }

        public ChannelReader<PubSubEventMessage> Reader => _channel.Reader;
        public ChannelWriter<PubSubEventMessage> Writer => _channel.Writer;
    }
}