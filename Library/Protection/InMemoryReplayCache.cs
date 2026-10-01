using System.Collections.Concurrent;

namespace SoEx.Protection;

public class InMemoryReplayCache : IMessageReplayCache
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _seen = new();
    private long _nextSweep;
    TimeProvider _timeProvider;

    public InMemoryReplayCache(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool TryAdd(string key, DateTimeOffset expires)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        Sweep(now);
        while (true)
        {
            if (_seen.TryAdd(key, expires))
            {
                return true;
            }

            if (_seen.TryGetValue(key, out DateTimeOffset existing) && existing > now)
            {
                return false;
            }

            if (_seen.TryUpdate(key, expires, existing))
            {
                return true;
            }
        }
    }

    public bool Seen(string key)
    {
        return _seen.TryGetValue(key, out DateTimeOffset expires) && expires > _timeProvider.GetUtcNow() ;
    }

    private void Sweep(DateTimeOffset now)
    {
        long ticks = now.UtcTicks;
        long next = Interlocked.Read(ref _nextSweep);
        if (ticks < next)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _nextSweep, ticks + TimeSpan.TicksPerSecond, next) != next)
        {
            return;
        }

        foreach (var entry in _seen)
        {
            if (entry.Value <= now)
            {
                _seen.TryRemove(entry);
            }
        }
    }
}
