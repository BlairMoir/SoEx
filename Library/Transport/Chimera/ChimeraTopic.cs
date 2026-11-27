using System.Collections.Concurrent;
using SoEx.Messaging.Chimera;

namespace SoEx.Transport.Chimera
{
    public class ChimeraTopic : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, Lazy<SharedTopicLog>> topics = new();

        public SqliteTopicLog For(ChimeraOptions options, string topic)
        {
            string path = Path.GetFullPath(options.RootDirectory);
            string identifier = $"{path}|{topic}";
            Lazy<SharedTopicLog> logTopic = topics.GetOrAdd(
                identifier,
                _ => new Lazy<SharedTopicLog>(() => SharedTopicLog.GetOrOpen(identifier, options, topic)));
            return logTopic.Value.Log;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (KeyValuePair<string, Lazy<SharedTopicLog>> topic in topics)
            {
                if (topics.TryRemove(topic.Key, out Lazy<SharedTopicLog>? topicLog) && topicLog.IsValueCreated)
                {
                    await topicLog.Value.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
