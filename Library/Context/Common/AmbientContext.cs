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

        private string ContextName<T>()
        {
            return typeof(T).FullName ?? typeof(T).Name;
        }

        public void SetIfNotExists<T>(Func<T> contextFactory) where T : notnull
        {
            string contextName = ContextName<T>();
            if (_contexts.ContainsKey(contextName))
            {
                return;
            }
            _contexts.TryAdd(contextName, contextFactory.Invoke());
        }

        public T Get<T>() where T : notnull
        {
            return (T)_contexts[ContextName<T>()];
        }

        public bool Contains<T>() where T : notnull
        {
            return _contexts.ContainsKey(ContextName<T>());
        }

        public void SetOrReplace<T>(T context) where T : notnull
        {
            string contextName = ContextName<T>();
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
            var outgoingBytes = new ConcurrentDictionary<string, byte[]>();
            foreach (var pair in _contexts)
            {
                outgoingBytes[pair.Key] = _messageSerializer.Serialize(pair.Value);
            }
            return _messageSerializer.Serialize(outgoingBytes);
        }

        public void Deserialize(byte[]? serializedContexts)
        {
            if (serializedContexts is null)
                return;

            ConcurrentDictionary<string, byte[]>? incomingBytes =
                _messageSerializer.Deserialize<ConcurrentDictionary<string, byte[]>>(serializedContexts);

            if (incomingBytes is null)
                return;

            foreach (var pair in incomingBytes)
            {
                object? value;
                try
                {
                    value = _messageSerializer.Deserialize<object>(pair.Value);
                }
                catch
                {
                    // silently drop any contexts that are not in our known types
                    continue;
                }

                if (value is null)
                    continue;

                _contexts[pair.Key] = value;
            }
        }
    }
}

