using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DaprDemo.D002.HelloGrpc
{
    public class LocalDaprClient : IHostedService
    {       
        public LocalDaprClient(ILogger<LocalDaprClient> logger, IHostApplicationLifetime hostApplicationLifetime)
        {            
            hostApplicationLifetime.ApplicationStarted.Register( async () => {                
            using var client = new DaprClientBuilder().Build();
            var response = await client.InvokeMethodGrpcAsync<HelloRequest, HelloReply>("DaprDemo-D002-HelloGrpc","sayhello",new HelloRequest(){ Name = "[Your name here]"});
            logger.LogInformation($"Grpc Response: {response.Message}");
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;        
        }
    }
}