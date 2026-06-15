namespace SoEx.Context
{
    public readonly record struct InvocationContext(Type contract, string MethodName);
    public readonly record struct EntryContext(InvocationContext entry);
}
