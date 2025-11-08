namespace SoEx.Context
{
    public interface IContextFlowPolicy
    {
        public void Incoming(IAmbientContext source, IAmbientContext destination);
        public void Outgoing(IAmbientContext source, IAmbientContext destination);
        public IDictionary<string, object> ScopeProperties(IAmbientContext context);
    }
}
