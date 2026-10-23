using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using SoEx.Abstractions;

namespace SoEx.Transport.InProc
{
    public class InProcListeners
    {
        ConcurrentDictionary<Uri, object> dictionary = new ConcurrentDictionary<Uri, object>();
        public InProcListeners()
        {

        }

        internal void Register<I>(Uri address, InProcEndpoint<I> endpoint) where I : class
        {
            if(!dictionary.TryAdd(address, endpoint))
            {
                throw new InvalidOperationException($"Address already registered {address}");
            }
        }

        internal void Deregister<I>(Uri address, InProcEndpoint<I> endpoint) where I : class
        {
           dictionary.TryRemove(new KeyValuePair<Uri, object>(address, endpoint));
        }

        public InProcEndpoint<I> ForAddress<I>(Uri address) where I : class
        {
            object? endpoint;
            if (dictionary.TryGetValue(address, out endpoint))
            {
                if (endpoint is InProcEndpoint<I> inProcEndpoint)
                {
                    return inProcEndpoint;
                }
            }
            throw new ArgumentException($"Address not registered {address}");
        }
    }
}
