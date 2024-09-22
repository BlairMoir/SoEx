using Grpc.Core;
using SoEx.Context;

namespace SoEx.Grpc.Context
{
    public class GrpcServiceResponseContext : IAmbientContext
    {
        private readonly Dictionary<string, object> _responseContext = [];

        public void UpdateResponseGrpcServiceContext(ServerCallContext serverCallContext)
        {
            Metadata headers = [];
            foreach (string key in _responseContext.Keys)
            {
                string keyName = AmbientGrpcContextHelper.KeyName(_responseContext[key].GetType());
                headers.Add(
                    keyName,
                    AmbientGrpcContextHelper.SerializeObject(_responseContext[key])
                );
            }
            serverCallContext.WriteResponseHeadersAsync(headers);
        }

        public bool Contains<T>() where T : class
        {
            throw new NotImplementedException();
        }
        public T Get<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public void SetIfNotExists<T>(Func<T> contextFactory) where T : class
        {
            string contextName = typeof(T).Name;
            if (_responseContext.ContainsKey(contextName))
            {
                return;
            }
            _responseContext.Add(contextName, contextFactory.Invoke());
        }

        public void SetOrReplace<T>(T context) where T : class
        {
            string contextName = typeof(T).Name;
            if (_responseContext.ContainsKey(contextName))
            {
                _responseContext[contextName] = context;
            }
            else
            {
                _responseContext.Add(contextName, context);
            }
        }
    }
}
