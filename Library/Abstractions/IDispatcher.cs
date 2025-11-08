namespace SoEx.Abstractions
{
    public interface IDispatcher
    {
        public Task<InvocationResponse> Dispatch<I>(InvocationRequest invocationRequest) where I : class;
    }
}
