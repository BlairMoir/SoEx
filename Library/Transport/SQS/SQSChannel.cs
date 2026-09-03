using System.Diagnostics;
using Amazon.SQS;
using Amazon.SQS.Model;
using SoEx.Topology;

namespace SoEx.Transport.SQS
{
    public class SQSChannel<I> : IChannel where I : class
    {
        AmazonSQSClient? _client;
        SQSBinding<I>? _sqsBinding;

        public Type Contract => typeof(I);

        public void Bind(Binding binding)
        {
            if (binding is SQSBinding<I> sqsBinding)
            {
                if (_sqsBinding is null)
                {
                    _sqsBinding = sqsBinding;
                    AmazonSQSConfig config = new AmazonSQSConfig();

                    if (!string.IsNullOrEmpty(_sqsBinding.Config.ServiceURL))
                    {
                        config.ServiceURL = _sqsBinding.Config.ServiceURL;
                    }

                    _client = new AmazonSQSClient(config);
                }
            }
        }

        public IPipeline? Pipeline => _sqsBinding?.Pipeline;


        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(SQSChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(_client is not null);
                    Debug.Assert(_sqsBinding is not null);

                    SendMessageRequest request = new SendMessageRequest
                    {
                        QueueUrl = _sqsBinding.Config.QueueUrl,
                        MessageBody = Convert.ToBase64String(payload),
                        MessageGroupId = typeof(I).FullName
                    };

                    await _client.SendMessageAsync(request).ConfigureAwait(false);
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
