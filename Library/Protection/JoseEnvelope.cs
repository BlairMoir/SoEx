using System.Security.Cryptography;
using System.Text;
using Jose;

namespace SoEx.Protection;

internal static class JoseEnvelope
{
    public const string Sender = "kid";
    public const string To = "to";
    public const string Action = "act";
    public const string Sent = "sent";
    public const string Nonce = "nonce";
    public const string InReplyTo = "re";
    public const string Key = "kid";

    public static string Text(ReadOnlySpan<byte> envelope)
    {
        foreach (var b in envelope)
        {
            if (b > 0x7F)
            {
                throw new MessageProtectionException(ProtectionFailure.Malformed);
            }
        }
        return Encoding.ASCII.GetString(envelope);
    }

    public static IDictionary<string, object> Headers(string token, int parts)
    {
        if (token.Split('.').Length != parts)
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }

        try
        {
            return JWT.Headers(token);
        }
        catch (Exception)
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }
    }

    public static string? String(IDictionary<string, object> headers, string name)
    {
        if (headers.TryGetValue(name, out object? value) && value is string text)
        {
            return text;
        }

        return null;
    }

    public static long Long(IDictionary<string, object> headers, string name)
    {
        if (headers.TryGetValue(name, out object? value) && value is long or int)
        {
            return Convert.ToInt64(value);
        }

        throw new MessageProtectionException(ProtectionFailure.Malformed);
    }

    public static Dictionary<string, object> SignedHeaders(string senderKeyId, string? action, TimeProvider time)
    {
        var headers = new Dictionary<string, Object>()
        {
            [Sender] = senderKeyId,
            [Sent] = time.GetUtcNow().ToUnixTimeMilliseconds(),
            [Nonce] = Convert.ToHexString(RandomNumberGenerator.GetBytes(16))
        };

        if (action is not null)
        {
            headers[Action] = action;
        }

        return headers;
    }

    public static DateTimeOffset Fresh(IDictionary<string, object> signedHeaders, TimeSpan window, TimeProvider time)
    {
        long milliseconds = Long(signedHeaders, Sent);
        if (milliseconds < 0 || milliseconds > DateTimeOffset.MaxValue.ToUnixTimeMilliseconds())
        {
            throw new MessageProtectionException(ProtectionFailure.Malformed);
        }

        DateTimeOffset sent = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        DateTimeOffset now = time.GetUtcNow();
        if (sent < now - window || sent > now + window)
        {
            throw new MessageProtectionException(ProtectionFailure.Stale);
        }

        return sent;
    }

    public static byte[] Bytes(string token)
    {
        return Encoding.ASCII.GetBytes(token);
    }
}
