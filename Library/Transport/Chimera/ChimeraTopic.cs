using System.Collections.Concurrent;
using SoEx.Messaging.Chimera;

namespace SoEx.Transport.Chimera;

public class ChimeraTopic : IAsyncDisposable
{
    private static readonly ConcurrentDictionary<string, SqliteTopicLog> s_topics = new ();

    public static SqliteTopicLog For(ChimeraOptions options, string topic)
    {
        string path = Path.GetFullPath(options.RootDirectory);
        string identifier = $"{path}|{topic}";
        var logTopic = s_topics.GetOrAdd(identifier, _ => SqliteTopicLog.Open(topic, options) );
        return logTopic;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var topic in s_topics)
        {
            s_topics.TryRemove(topic.Key, out _);
            await topic.Value.DisposeAsync();
        }
    }
}
