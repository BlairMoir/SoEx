using System.Collections.Concurrent;

namespace SoEx.Context
{
    public class AmbientContext : IAmbientContext
    {
        protected readonly ConcurrentDictionary<string, object> _contexts = [];

        public void SetIfNotExists<T>(Func<T> contextFactory) where T : class
        {
            string contextName = typeof(T).Name;
            if (_contexts.ContainsKey(contextName))
            {
                return;
            }
            _contexts.TryAdd(contextName, contextFactory.Invoke());
        }

        public T Get<T>() where T : class
        {
            return (T)_contexts[typeof(T).Name];
        }

        public bool Contains<T>() where T : class
        {
            return _contexts.ContainsKey(typeof(T).Name);
        }

        public void SetOrReplace<T>(T context) where T : class
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
    }
}
