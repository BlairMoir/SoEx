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
            var backwardsCompat = new ConcurrentDictionary<string, object>();
            foreach (object value in _contexts.Values)
            {
                Type type = value.GetType();
                string fullName = type.FullName ?? type.Name;
                if (fullName != type.Name)
                {
                    backwardsCompat[fullName] = value;
                }
                backwardsCompat[type.Name] = value;
            }
            return _messageSerializer.Serialize(backwardsCompat);
        }

        public void Deserialize(byte[]? serializedContexts)
        {
            if (serializedContexts is null)
                return;


            ConcurrentDictionary<string, object>? incoming = _messageSerializer.Deserialize<ConcurrentDictionary<string, object>>(serializedContexts);

            if (incoming is null)
                return;


            foreach (var replacement in incoming)
            {
                // backwards compat - can just use the key once all in flight messages are upgraded.
                Type type = replacement.Value.GetType();
                _contexts[type.FullName ?? type.Name] = replacement.Value;
            }
        }
    }
}

