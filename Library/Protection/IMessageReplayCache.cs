namespace SoEx.Protection;

public interface IMessageReplayCache
{
    bool TryAdd(string key, DateTimeOffset expires);

    bool Seen(string key);
}
