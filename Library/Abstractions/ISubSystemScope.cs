namespace SoEx.Abstractions
{
    public interface ISubSystemScope
    {
        public ISubSystemHost For<I>();
    }
}
