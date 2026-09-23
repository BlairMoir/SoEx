using System.Collections.Immutable;


namespace SoEx.Topology;

public abstract record Address(Uri Uri)
{
    public virtual ImmutableArray<Uri> Uris => [Uri];

    public record Single(Uri Uri) : Address(Uri) { }

    public record Many(params ImmutableArray<Uri> Uris) : Address(Uris[0])
    {
        public override ImmutableArray<Uri> Uris { get; } = Uris;
    }
};
