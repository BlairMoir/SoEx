using System.Collections.Concurrent;
using SoEx.Abstractions;

namespace SoEx.Context
{
    public class AmbientContext : IAmbientContext
    {
        readonly IMessageSerializer _messageSerializer;
        protected readonly ConcurrentDictionary<string, object> _contexts = [];

        public AmbientContext(IMessageSerializer messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public void SetIfNotExists<T>(Func<T> contextFactory) where T : notnull
        {
            string contextName = typeof(T).Name;
            if (_contexts.ContainsKey(contextName))
            {
                return;
            }
            _contexts.TryAdd(contextName, contextFactory.Invoke());
        }

        public T Get<T>() where T : notnull
        {
            return (T)_contexts[typeof(T).Name];
        }

        public bool Contains<T>() where T : notnull
        {
            return _contexts.ContainsKey(typeof(T).Name);
        }

        public void SetOrReplace<T>(T context) where T : notnull
        {
            string contextName = typeof(T).Name;
            if (_contexts.ContainsKey(contextName))
            {
                _contexts[contextName] = context;
            }
            else
            {
                _contexts.TryAdd(contextName, context);
            }
        }

        public byte[] Serialize()
        {
            return _messageSerializer.Serialize(_contexts);
        }

        public void Deserialize(byte[]? serlializedContexts)
        {
            if (serlializedContexts is null)
                return;


            ConcurrentDictionary<string, object>? replacmentContexts = _messageSerializer.Deserialize<ConcurrentDictionary<string, object>>(serlializedContexts);

            if (replacmentContexts is null)
                return;


            foreach (var replacement in replacmentContexts)
            {
                _contexts.AddOrUpdate(replacement.Key, replacement.Value, (k, v) => replacement.Value);
            }
        }
    }
}

