using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using SoEx.Abstractions;
using SoEx.Topology;

namespace SoEx.Relay
{
    public class RelayChannel<I> : IChannel
    {
        RelayBinding<I>? _relayBinding;
        HttpClient _httpClient = new HttpClient();
        readonly IMessageSerializer _serializer;

        public RelayChannel(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void Bind(Binding binding)
        {
            if (binding is RelayBinding<I> relayBinding)
            {
                _relayBinding = relayBinding;
            }
        }

        public IPipeline? Pipeline => _relayBinding?.Pipeline;

        public async Task<byte[]> InvokeResult(byte[] payload)
        {
            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Client.StartActivity($"{nameof(RelayChannel<I>)}"))
            {
                try
                {
                    Debug.Assert(_relayBinding is not null);
                    var tokenProvider = _relayBinding.CreateTokenProvider();
                    var uri = new Uri(_relayBinding.Transport.Address.OriginalString.Replace("sb://", "https://"));
                    var token = tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1)).Result.TokenString;

                    var httpRequest = new HttpRequestMessage()
                    {
                        RequestUri = uri,
                        Method = HttpMethod.Get,
                        Content = new ByteArrayContent(payload)
                    };
                    httpRequest.Headers.Add("ServiceBusAuthorization", token);
                    var response =await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var responseBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    return responseBytes;
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
