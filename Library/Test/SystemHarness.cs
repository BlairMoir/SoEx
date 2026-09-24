using Autofac;
using SoEx.Messaging.Chimera;
using SoEx.Messaging.Chimera.Testing;

namespace SoEx.Test;

public sealed class SystemHarness
{
    private readonly ILifetimeScope _container;
    private readonly ChimeraOptions? _chimeraOptions;

    internal SystemHarness(ILifetimeScope container, ChimeraOptions? chimeraOptions = null)
    {
        _container = container;
        _chimeraOptions = chimeraOptions;
    }

    public async Task Proxy<I>(Func<I, Task> callerFunc) where I : notnull
    {
        using (var scope = _container.BeginLifetimeScopeAsyncLocal())
        {
            await callerFunc.Invoke(scope.Resolve<I>());
        }
    }

    public async Task<TResult> Proxy<I,TResult>(Func<I, Task<TResult>> callerFunc) where I : notnull
    {
        using (var scope = _container.BeginLifetimeScopeAsyncLocal())
        {
            return await callerFunc.Invoke(scope.Resolve<I>());
        }
    }

    public async Task WaitForEvents(TimeSpan? timeout = null)
    {
        if (_chimeraOptions is null)
            return;

        timeout ??= TimeSpan.FromSeconds(10);
        var deadline = DateTime.UtcNow + timeout;
        int quietReads = 0;


        while (true)
        {
            var chimeraOverview = ChimeraInspector.Overview(_chimeraOptions);
            quietReads = chimeraOverview.Settled ? quietReads + 1 : 0;
            if (quietReads == 2)
            {
                if (chimeraOverview.DeadLettered > 0)
                    throw new InvalidOperationException(
                        Describe("Event handlers failed.",
                            chimeraOverview.Subscribers,
                            s=> s.DeadLettered > 0));

                return;
            }

            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException(
                    Describe($"Events did not settle within: {timeout}",
                        chimeraOverview.Subscribers,
                        s => !s.Settled));
            }
            await Task.Delay(25);
        }
    }

    private static string Describe(string message, IEnumerable<SubscriberActivity> activities, Func<SubscriberActivity, bool> filter)
    {
        var failures = activities.Where(filter).Select( a=>
            $"{a.Subscriber} on {a.Topic}: behind {a.Behind}, in flight {a.InFlight}, " +
            $"awaiting retry {a.AwaitingRetry}, dead lettered {a.DeadLettered}");

        return $"{message} {string.Join("; ", failures)}";
    }
}
