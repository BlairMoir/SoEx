namespace SoEx.Abstractions
{
    public interface IMessageSerializer
    {
        public T? Deserialize<T>(byte[] payload);
        public byte[] Serialize<T>(T? @object);
    }
}
