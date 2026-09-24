using System.Collections.Concurrent;
using SoEx.Messaging.Chimera;

namespace SoEx.Transport.Chimera;

public class ChimeraTopic : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, SqliteTopicLog> topics = new ();

    public SqliteTopicLog For(ChimeraOptions options, string topic)
    {
        string path = Path.GetFullPath(options.RootDirectory);
        string identifier = $"{path}|{topic}";
        var logTopic = topics.GetOrAdd(identifier, _ => SqliteTopicLog.Open(topic, options) );
        return logTopic;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var topic in topics)
        {
            topics.TryRemove(topic.Key, out _);
            await topic.Value.DisposeAsync();
        }
    }
}
