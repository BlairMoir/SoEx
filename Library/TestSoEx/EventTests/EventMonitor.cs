using System.Collections.Concurrent;

namespace SoEx.TestSoEx.EventTests
{
    public interface IEventMonitor
    {
        Task WaitForEventAsync(string eventName, TimeSpan timeout);
        void RecordEvent(string eventName);
    }

    public class EventMonitor : IEventMonitor
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _eventSources = new();
        public void RecordEvent(string eventName)
        {
            TaskCompletionSource<bool> tcs = _eventSources.GetOrAdd(eventName, _ => new TaskCompletionSource<bool>());
            if (!tcs.TrySetResult(true))
            {
                throw new Exception("failed to set task completion source");
            }
        }
        public async Task WaitForEventAsync(string eventName, TimeSpan timeout)
        {
            TaskCompletionSource<bool> tcs = _eventSources.GetOrAdd(eventName, _ => new TaskCompletionSource<bool>());
            await tcs.Task.WaitAsync(timeout);
        }
    }
}
