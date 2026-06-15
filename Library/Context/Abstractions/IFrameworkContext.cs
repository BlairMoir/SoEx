// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace SoEx.Context;

public interface IFrameworkContext
{
    public T Get<T>() where T : struct;
    public bool Contains<T>() where T : struct;
}
