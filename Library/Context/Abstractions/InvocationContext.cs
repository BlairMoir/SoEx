namespace SoEx.Context
{
    public readonly record struct InvocationContext(string Contract, string MethodName);
    public readonly record struct EntryContext(InvocationContext Entry);
    public readonly record struct PreviousEntryContext(InvocationContext Entry);
}
