namespace SoEx.Transport.InProc
{
    public record InProcTransport : Topology.Transport
    {
        public InProcTransport()
        {
            ClientChannel = typeof(InProcChannel<>);
            HostChannel = typeof(InProcEndpoint<>);
        }
    }
}
