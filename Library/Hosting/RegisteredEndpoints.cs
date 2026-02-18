using SoEx.Topology;

namespace SoEx.Hosting
{
    public class RegisteredEndpoints
    {
        List<IEndpoint> _endpoints = new List<IEndpoint>();

        public IEndpoint[] Endpoints => _endpoints.ToArray();

        public void AddEndpoint(IEndpoint endpoint)
        {
            _endpoints.Add(endpoint);
        }
    }
}
