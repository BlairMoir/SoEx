using SoEx.Messaging.Chimera;

namespace SoEx.Transport.Chimera
{
    internal sealed class SharedTopicLog : IAsyncDisposable
    {
        private static readonly Dictionary<string, SharedTopicLog> s_topics = new Dictionary<string, SharedTopicLog>();
        private static readonly Lock s_topicLock = new Lock();

        private readonly string _identifier;

        public SqliteTopicLog Log { get; }
        private int Users { get; set; }

        private SharedTopicLog(string identifier, SqliteTopicLog log)
        {
            _identifier = identifier;
            Log = log;
        }

        public static SharedTopicLog GetOrOpen(string identifier, ChimeraOptions options, string topic)
        {
            lock (s_topicLock)
            {
                if (!s_topics.TryGetValue(identifier, out SharedTopicLog? sharedTopicLog))
                {
                    sharedTopicLog = new SharedTopicLog(identifier, SqliteTopicLog.Open(topic, options));
                    s_topics.Add(identifier, sharedTopicLog);
                }

                sharedTopicLog.Users++;
                return sharedTopicLog;
            }
        }

        public async ValueTask DisposeAsync()
        {
            lock (s_topicLock)
            {
                Users--;

                if (Users > 0)
                    return;

                s_topics.Remove(_identifier);
            }
            await Log.DisposeAsync().ConfigureAwait(false);
        }
    }
}
