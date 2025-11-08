namespace SoEx.Abstractions
{
    public interface IHostAndClientLookup
    {
        public ISubSystemHost For(Type type);
        public ISubSystemHost For<I>();
        public ISubSystemHost[] SubSystemHosts { get; }
    }
}
