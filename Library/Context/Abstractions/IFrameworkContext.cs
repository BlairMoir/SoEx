namespace SoEx.Context
{
    public interface IFrameworkContext
    {
        public InvocationContext Invocation { get; }
        public EntryContext? Entry { get; }
        public PreviousEntryContext? Previous { get; }

    }
}
