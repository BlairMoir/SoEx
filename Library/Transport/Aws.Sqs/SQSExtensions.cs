using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Transport.Aws.Sqs
{
    public static class SQSExtensions
    {
        public static void SQSClient(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(SQSChannel<>), typeof(SQSChannel<>));
        }
    }
}
