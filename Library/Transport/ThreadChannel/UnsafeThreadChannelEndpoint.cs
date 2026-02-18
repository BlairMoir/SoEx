using System.Diagnostics;
using System.Threading.Channels;
using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.ThreadChannel
{
    public class UnsafeThreadChannelEndpoint<I> : IEndpoint where I : class
    {
        ILogger<UnsafeThreadChannelEndpoint<I>> _logger;
        ChannelReader<byte[]> _reader;
        UnsafeThreadChannelBinding<I>? _unsafeThreadChannelBinding;
        IEndpointPipeline _endpointPipeLine;


        public UnsafeThreadChannelEndpoint(ILogger<UnsafeThreadChannelEndpoint<I>> logger, IEndpointPipeline endpointPipeLine, UnsafeThreadEventChannel<I> channel)
        {
            _logger = logger;

            _reader = channel.Reader;
            _endpointPipeLine = endpointPipeLine;

        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is UnsafeThreadChannelBinding<I> unsafeThreadChannelBinding)
            {
                _unsafeThreadChannelBinding = unsafeThreadChannelBinding;
            }
        }


        public Task Listen()
        {
            Task.Factory.StartNew(ReadChannel, TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        private async Task ReadChannel()
        {
            while (await _reader.WaitToReadAsync())
            {
                var request = await _reader.ReadAsync();
                await Dispatch(request);
            }
        }

        private async Task Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(UnsafeThreadChannelEndpoint<I>)}", ActivityKind.Server))
            {
                try
                {
                    await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _unsafeThreadChannelBinding?.Pipeline, activity);

                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
                }
            }
        }
    }
}
