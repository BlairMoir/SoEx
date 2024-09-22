using System.Text;
using Grpc.Core;
using Newtonsoft.Json;

namespace SoEx.Grpc.Context
{
    public static class AmbientGrpcContextHelper
    {
        public static string KeyName<T>() where T : class
        {
            return KeyName(typeof(T));
        }

        public static string KeyName(Type t)
        {
            return $"soex_{t.Name}{Metadata.BinaryHeaderSuffix}".ToLowerInvariant();
        }

        public static byte[] SerializeObject(object? value)
        {
            string contextJson = JsonConvert.SerializeObject(value);
            byte[] contextBytes = Encoding.UTF8.GetBytes(contextJson);
            return contextBytes;
        }

        public static T DeserializeObject<T>(byte[] bytes) where T : class
        {
            string jsonstring = Encoding.UTF8.GetString(bytes);
            T? contextObject = JsonConvert.DeserializeObject<T>(jsonstring);
            ArgumentNullException.ThrowIfNull(contextObject);
            return contextObject;
        }
    }
}
