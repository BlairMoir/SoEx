namespace SoEx.Topology
{
    public interface IEndpoint
    {
        public Task Listen();
        public Task Close();
        public void Bind(Binding binding, string componentName);
    }
}
