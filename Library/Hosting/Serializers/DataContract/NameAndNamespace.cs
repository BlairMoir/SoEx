// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace SoEx.Hosting.Serializers.DataContract;

public record NameAndNamespace
{
    public required string Name { get; init; }
    public required string Namespace { get; init; }
}
