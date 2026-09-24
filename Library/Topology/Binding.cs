namespace SoEx.Topology
{
    public abstract record Binding
    {
        protected Binding(Type contract, Transport transport, string subSystem)
        {
            Contract = contract;
            Transport = transport;
            SubSystem = subSystem;
        }

        public Type Contract { get; }
        public Transport Transport { get; }
        public string SubSystem { get;  }
        public IBindingPipeline? Pipeline { get; init; }
    }
}
