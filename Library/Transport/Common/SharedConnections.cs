using System.Collections.Concurrent;

namespace SoEx.Transport;

public sealed class SharedConnections<TConnection> : IAsyncDisposable where TConnection : class
{
    readonly ConcurrentDictionary<string, Lazy<Task<TConnection>>> _connections = new();
    int _disposed;

    public Task<TConnection> Get(string key, Func<Task<TConnection>> open, Func<TConnection, bool> isFaulted)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) == 1, this);

        var entry = _connections.GetOrAdd(key, _ => new Lazy<Task<TConnection>>(() => Open(open)));
        var task = entry.Value;

        if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
        {
            _connections.TryRemove(new KeyValuePair<string, Lazy<Task<TConnection>>>(key, entry));
            _ = DisposeWhenOpen(task);
            throw new ObjectDisposedException(GetType().FullName);
        }


        if (!task.IsCompleted || (task.IsCompletedSuccessfully && !isFaulted.Invoke(task.Result)))
            return task;

        _connections.TryRemove(new KeyValuePair<string, Lazy<Task<TConnection>>>(key, entry));

        if (!task.IsCompletedSuccessfully)
            return task;

        _ = DisposeQuietly(task.Result);
        var replacement = _connections.GetOrAdd(key, _ => new Lazy<Task<TConnection>>(() => Open(open)));
        return replacement.Value;
    }

    private static async Task<TConnection> Open(Func<Task<TConnection>> open) => await open().ConfigureAwait(false);

    private static async Task DisposeQuietly(TConnection connection)
    {
        try
        {
            switch (connection)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
        catch
        {
            // failing to dispose does not affect the caller
        }
    }

    static async Task DisposeWhenOpen(Task<TConnection> task)
    {
        try
        {
            await DisposeQuietly(await task.ConfigureAwait(false)).ConfigureAwait(false);
        }
        catch
        {
            // nothing to close
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        foreach (var key in _connections.Keys)
        {
            if(_connections.TryRemove(key, out var entry) && entry.IsValueCreated )
                await DisposeWhenOpen(entry.Value).ConfigureAwait(false);
        }
    }
}
