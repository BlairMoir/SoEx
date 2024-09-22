namespace SoEx.Context
{
    public abstract class ServiceContext
    {
        public string Name { get; init; }

        public ServiceContext(string name)
        {
            Name = name;
        }
    }
}
