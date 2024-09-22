using Grpc.Core;
using SoEx.Context;

namespace SoEx.Grpc.Context
{
    public class GrpcClientResponseContext : IAmbientContext
    {
        private readonly Metadata _headers;

        public GrpcClientResponseContext(Metadata headers)
        {
            _headers = headers;
        }

        public bool Contains<T>() where T : class
        {
            bool contains = _headers
                .Any(x => string.CompareOrdinal(x.Key, AmbientGrpcContextHelper.KeyName<T>()) == 0);
            return contains;
        }

        public T Get<T>() where T : class
        {
            Metadata.Entry? contextEntry = _headers
                .FirstOrDefault(x => string.CompareOrdinal(x.Key, AmbientGrpcContextHelper.KeyName<T>()) == 0);

            ArgumentNullException.ThrowIfNull(contextEntry);
            return AmbientGrpcContextHelper.DeserializeObject<T>(contextEntry.ValueBytes);
        }

        public void SetIfNotExists<T>(Func<T> contextFactory) where T : class
        {
            throw new NotImplementedException();
        }

        public void SetOrReplace<T>(T context) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
