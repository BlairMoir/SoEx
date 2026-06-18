// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace SoEx.Context;

public interface IFrameworkContext
{
    public InvocationContext Invocation { get; }
    public EntryContext? Entry { get; }
    public PreviousEntryContext? Previous { get; }

}
