using Grpc.Core;
using Grpc.Core.Interceptors;
using SoEx.Context;

namespace SoEx.Grpc.Context
{
    public class GrpcClientRequestContext : IAmbientContext
    {
        private Dictionary<string, object> _outgoingClientContext = new Dictionary<string, object>();

        public void UpdateOutgoingGrpcClientContext<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> grpcContext)
            where TRequest : class
            where TResponse : class
        {
            Metadata headers = grpcContext.Options.Headers ?? new Metadata();
            foreach (var outgoingContextPair in _outgoingClientContext)
            {
                object outgoingContext = outgoingContextPair.Value;
                Metadata.Entry? contextEntry = headers
                 .FirstOrDefault(x => string.CompareOrdinal(x.Key, AmbientGrpcContextHelper.KeyName(outgoingContext.GetType())) == 0);

                if (contextEntry is null)
                {
                    headers.Add(
                        AmbientGrpcContextHelper.KeyName(
                            outgoingContext.GetType()),
                            AmbientGrpcContextHelper.SerializeObject(outgoingContext)
                        );
                }
                else
                {
                    throw new NotImplementedException("This shouldn't happen!");
                }
            }
            CallOptions updatedOptions = grpcContext.Options.WithHeaders(headers);
            grpcContext = new ClientInterceptorContext<TRequest, TResponse>(grpcContext.Method, grpcContext.Host, updatedOptions);
        }

        public bool Contains<T>() where T : class
        {
            return _outgoingClientContext.ContainsKey(typeof(T).Name);
        }

        public T Get<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public void SetIfNotExists<T>(Func<T> contextFactory) where T : class
        {
            string contextName = typeof(T).Name;
            if (_outgoingClientContext.ContainsKey(contextName))
            {
                return;
            }
            _outgoingClientContext.Add(contextName, contextFactory.Invoke());
        }

        public void SetOrReplace<T>(T context) where T : class
        {
            var contextName = typeof(T).Name;
            if (_outgoingClientContext.ContainsKey(contextName))
            {
                _outgoingClientContext[contextName] = context;
            }
            else
            {
                _outgoingClientContext.Add(contextName, context);
            }
        }
    }
}
