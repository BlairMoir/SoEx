// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Cryptography.X509Certificates;

namespace SoEx.Transport.Grpc.Protection;

public class GrpcCertificate : GrpcProtection
{
    public required X509Certificate2 Certificate { get; init; }
}
