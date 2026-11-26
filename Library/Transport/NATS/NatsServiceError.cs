using NATS.Client.Core;

namespace SoEx.Transport.NATS
{
    internal class NatsServiceError
    {
        private const string ErrorHeader = "Nats-Service-Error";
        private const string ErrorHeaderCode = "Nats-Service-Error-Code";

        public static NatsHeaders Headers()
        {
            var headers = new NatsHeaders
            {
                { ErrorHeader, "service failed" },
                { ErrorHeaderCode, "500" }
            };
            return headers;
        }

        public static bool IsIn(NatsMsg<byte[]> reply)
        {
            if (reply.Headers is null)
                return false;

            return reply.Headers.ContainsKey(ErrorHeader);
        }
    }
}
