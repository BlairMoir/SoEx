using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.SQS
{
    public static class SQSExtensions
    {
        public static void SQSClient(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(SQSChannel<>), typeof(SQSChannel<>));
        }
    }
}
