using System.Diagnostics;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using SoEx.Endpoint;
using SoEx.Topology;

namespace SoEx.Transport.SQS
{
    public class SQSEndpoint<I> : IEndpoint where I : class
    {
        readonly ILogger<SQSEndpoint<I>> _logger;
        readonly IEndpointPipeline _endpointPipeLine;
        SQSBinding<I>? _sqsBinding;
        AmazonSQSClient? _client;
        CancellationTokenSource? _cancellationTokenSource;
        Task? _pollingTask;

        public SQSEndpoint(ILogger<SQSEndpoint<I>> logger, IEndpointPipeline endpointPipeLine)
        {
            _logger = logger;
            _endpointPipeLine = endpointPipeLine;
        }

        public void Bind(Binding binding, string componentName)
        {
            if (binding is SQSBinding<I> sqsBinding)
            {
                _sqsBinding = sqsBinding;

                if (_sqsBinding.Config.MaxNumberOfMessages < 1 || _sqsBinding.Config.MaxNumberOfMessages > 10)
                {
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(binding)}.{nameof(SQSBinding<I>.Config)}.{nameof(SQSConfig.MaxNumberOfMessages)}",
                        _sqsBinding.Config.MaxNumberOfMessages,
                        "MaxNumberOfMessages must be between 1 and 10 (AWS SQS limit)");
                }

                if (_sqsBinding.Config.WaitTimeSeconds < 0 || _sqsBinding.Config.WaitTimeSeconds > 20)
                {
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(binding)}.{nameof(SQSBinding<I>.Config)}.{nameof(SQSConfig.WaitTimeSeconds)}",
                        _sqsBinding.Config.WaitTimeSeconds,
                        "WaitTimeSeconds must be between 0 and 20 seconds (AWS SQS limit)");
                }

                AmazonSQSConfig config = new AmazonSQSConfig();

                if (!string.IsNullOrEmpty(_sqsBinding.Config.ServiceURL))
                {
                    config.ServiceURL = _sqsBinding.Config.ServiceURL;
                }

                _client = new AmazonSQSClient(config);
            }
        }

        public async Task Listen()
        {
            ArgumentNullException.ThrowIfNull(_client, "Binding must be set before listening");
            ArgumentNullException.ThrowIfNull(_sqsBinding, "Binding must be set before listening");

            _cancellationTokenSource = new CancellationTokenSource();
            _pollingTask = Task.Factory.StartNew(async () => await PollMessages(_cancellationTokenSource.Token),
                TaskCreationOptions.LongRunning);

            await Task.CompletedTask;
        }

        public async Task Close()
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();

                if (_pollingTask is not null)
                {
                    try
                    {
                        await _pollingTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancelling
                    }
                }

                _cancellationTokenSource.Dispose();
            }
        }

        private async Task PollMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Debug.Assert(_client is not null);
                    Debug.Assert(_sqsBinding is not null);

                    ReceiveMessageRequest request = new ReceiveMessageRequest
                    {
                        QueueUrl = _sqsBinding.Config.QueueUrl,
                        MaxNumberOfMessages = _sqsBinding.Config.MaxNumberOfMessages,
                        WaitTimeSeconds = _sqsBinding.Config.WaitTimeSeconds
                    };
                    ReceiveMessageResponse response = await _client.ReceiveMessageAsync(request, cancellationToken);

                    if (response.Messages != null)
                    {
                        foreach (Message? message in response.Messages)
                        {
                            try
                            {
                                await MessageHandler(message);
                                await _client.DeleteMessageAsync(
                                    _sqsBinding.Config.QueueUrl,
                                    message.ReceiptHandle,
                                    cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing message: {MessageId}", message.MessageId);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in polling loop");
                    // Brief delay before retrying to avoid tight loop on persistent errors
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private async Task MessageHandler(Message message)
        {
            byte[] payload = Convert.FromBase64String(message.Body);
            await Dispatch(payload);
        }

        private async Task Dispatch(byte[] serializedRequest)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{typeof(SQSEndpoint<I>)}", ActivityKind.Server))
            {
                try
                {
                    await _endpointPipeLine.ServicePipeLine<I>(serializedRequest, _sqsBinding?.Pipeline, activity);
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
                    throw;
                }
            }
        }
    }
}
