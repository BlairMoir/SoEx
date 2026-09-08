using System.Diagnostics.CodeAnalysis;
using System.Security;

namespace SoEx.Topology;

public abstract record Address(Uri Uri)
{
    public virtual Uri[] Uris => [Uri];

    public record Single(Uri Uri) : Address(Uri) { }

    public record Many(params Uri[] Uris) : Address(Uris[0])
    {
        public override Uri[] Uris { get; } = Uris;
    }
};
