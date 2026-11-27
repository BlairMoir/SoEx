using System.Collections.Concurrent;
using System.Diagnostics;

namespace SoEx.Hosting.Serializers.InProcReference
{
    internal static class InProcReferences
    {
        static readonly ConcurrentDictionary<Guid, (object? Value, long Timestamp)> References = new();
        private static long _nextSweep;
        private static readonly long s_ttl = Stopwatch.Frequency * 60;

        public static byte[] Put(object? @object)
        {
            var id = Guid.NewGuid();
            References[id] = (@object, Stopwatch.GetTimestamp());
            CheckTTL();
            return id.ToByteArray();
        }

        private static void CheckTTL()
        {
            long next = Volatile.Read(ref _nextSweep);
            long now = Stopwatch.GetTimestamp();
            if (now >= next && Interlocked.CompareExchange(ref _nextSweep, now + s_ttl, next) == next)
            {
                Sweep(now);
            }
        }

        private static void Sweep(long now)
        {
            foreach ((Guid id, (object? Value, long Timestamp) entry) in References)
            {
                if (now - entry.Timestamp > s_ttl)
                {
                    References.TryRemove(id, out _);
                }
            }
        }

        public static T? Take<T>(byte[] bytes)
        {
            if (bytes.Length != 16)
                throw new InvalidOperationException("Reference payload not found");

            Guid id = new Guid(bytes);
            if (!References.TryRemove(id, out (object? Value, long Timestamp) reference))
            {
                throw new InvalidOperationException($"Reference payload {id} not found");
            }


            return (T?)reference.Value;
        }
    }
}
