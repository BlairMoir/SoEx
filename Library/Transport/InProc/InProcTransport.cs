namespace SoEx.Transport.InProc
{
    public class InProcTransport : Topology.Transport
    {
        public InProcTransport()
        {
            ClientChannel = typeof(InProcChannel<>);
            HostChannel = typeof(InProcEndpoint<>);
        }
    }
}
