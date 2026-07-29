namespace SoEx.Hosting.Serializers.DataContract;

public record NameAndNamespace
{
    public required string Name { get; init; }
    public required string Namespace { get; init; }
}
