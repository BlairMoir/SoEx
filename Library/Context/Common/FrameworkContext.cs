using System.Collections.Concurrent;
using SoEx.Abstractions;

namespace SoEx.Context
{
    public class FrameworkContext : IFrameworkContext
    {
        readonly IMessageSerializer _messageSerializer;
        readonly ConcurrentDictionary<string, object> _contexts = [];

        public FrameworkContext(IMessageSerializer messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public void SetIfNotExists<T>(Func<T> contextFactory) where T : struct
        {
            string contextName = ContextName<T>();
            if (_contexts.ContainsKey(contextName))
            {
                return;
            }
            _contexts.TryAdd(contextName, contextFactory.Invoke());
        }

        public T Get<T>() where T : struct
        {
            return (T)_contexts[ContextName<T>()];
        }

        public bool Contains<T>() where T : struct
        {
            return _contexts.ContainsKey(ContextName<T>());
        }

        public void SetOrReplace<T>(T context) where T : struct
        {
            string contextName = ContextName<T>();
            _contexts[contextName] = context;
        }

        private string ContextName<T>()
        {
            return typeof(T).FullName ?? typeof(T).Name;
        }

        public byte[] Serialize()
        {
            return _messageSerializer.Serialize(_contexts);
        }

        public void Deserialize(byte[]? serlializedContexts)
        {
            if (serlializedContexts is null)
                return;

            ConcurrentDictionary<string, object>? replacementContexts = _messageSerializer.Deserialize<ConcurrentDictionary<string, object>>(serlializedContexts);

            if (replacementContexts is null)
                return;

            foreach (var replacement in replacementContexts)
            {
                _contexts[replacement.Key] = replacement.Value;
            }
        }

        public InvocationContext Invocation => Get<InvocationContext>();
        public EntryContext? Entry => Contains<EntryContext>() ? Get<EntryContext>() : null;
        public PreviousEntryContext? Previous => Contains<PreviousEntryContext>() ? Get<PreviousEntryContext>() : null;
    }
}

