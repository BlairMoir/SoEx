using System.Runtime.CompilerServices;
using NATS.Client.Core;

namespace SoEx.Transport.NATS
{
    internal static class NatsClientKeyExtensions
    {
        private static readonly ConditionalWeakTable<NatsOpts, string> s_ids = new();
        private static long s_next;

        public static string ClientKey(this NatsConfig config)
        {
            string id = s_ids.GetValue(config.Options, _ => Interlocked.Increment(ref s_next).ToString());
            return $"{config.Url}:{id}";
        }
    }
}
