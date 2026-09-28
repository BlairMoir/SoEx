using System.Collections.Concurrent;
using System.Diagnostics;
using SoEx.Abstractions;

namespace SoEx.Hosting.Serializers.InProcReference;

public class InProcReferenceSerializer : IMessageSerializer
{
    static readonly ConcurrentDictionary<Guid,(object? Value, long Timestamp)> References = new();
    private static long _nextSweep;
    private static readonly long s_ttl = Stopwatch.Frequency * 60;

    public T? Deserialize<T>(byte[] payload)
    {
        return Take<T>(payload);
    }

    public T? Deserialize<T>(byte[] payload, Type contractType, string? methodName = null)
    {
        return Deserialize<T>(payload);
    }

    public byte[] Serialize<T>(T? @object){

        return Put(Freeze(@object));
    }

    public byte[] Serialize<T>(T? @object, Type contractType, string methodName)
    {
        return Serialize(@object);
    }

    static object? Freeze(object? @object) => @object switch
    {
        InvocationRequest r => new InvocationRequest()
        {
            ActivityId = r.ActivityId,
            HasResult = r.HasResult,
            MethodName = r.MethodName,
            Arguments = Immutability.Clone(Immutability.Require(r.Arguments)),
            AmbientContext = (byte[]?)r.AmbientContext?.Clone(),
            FrameworkContext =  (byte[]?)r.FrameworkContext?.Clone(),
        },
        InvocationResponse r => new InvocationResponse()
        {
            Response = Immutability.Require(r.Response),
            AmbientContext = (byte[]?)r.AmbientContext?.Clone(),
        },
        _ => Immutability.Require(@object)
    };

    public byte[] Put(object? @object)
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
        foreach(var (id, entry) in References)
        {
            if(now - entry.Timestamp > s_ttl)
            {
                References.TryRemove(id, out _);
            }
        }
    }

    public T? Take<T>(byte[] bytes)
    {
        if (bytes.Length != 16 || !References.TryRemove(new Guid(@bytes), out var reference))
            throw new InvalidOperationException("Reference payload not found");
        return (T?)reference.Value;
    }
}
