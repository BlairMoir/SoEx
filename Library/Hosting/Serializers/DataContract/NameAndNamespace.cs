namespace SoEx.Hosting.Serializers.DataContract
{
    internal record NameAndNamespace
    {
        public required string Name { get; init; }
        public required string Namespace { get; init; }
    }
}
