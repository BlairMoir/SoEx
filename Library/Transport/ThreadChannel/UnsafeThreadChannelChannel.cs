using System.Diagnostics;
using System.Threading.Channels;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Transport.ThreadChannel
{
    public class UnsafeThreadChannelChannel<I> : IChannel where I : class
    {
        readonly IMessageSerializer _serializer;
        private readonly ChannelWriter<byte[]> _writer;
        UnsafeThreadChannelChannel<I>? _unsafeThreadChannelChannel;

        public UnsafeThreadChannelChannel(IMessageSerializer serializer, UnsafeThreadEventChannel<I> channel)
        {
            _serializer = serializer;
            _writer = channel.Writer;
        }

        public void Bind(Binding binding)
        {
            if (binding is UnsafeThreadChannelChannel<I> unsafeThreadChannelChannel)
            {
                _unsafeThreadChannelChannel = unsafeThreadChannelChannel;
            }
        }

        public IPipeline? Pipeline => _unsafeThreadChannelChannel?.Pipeline;


        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(UnsafeThreadChannelBinding<I>)}"))
            {
                try
                {
                    await _writer.WriteAsync(payload);
                    return [];
                }
                catch
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

    }
}
