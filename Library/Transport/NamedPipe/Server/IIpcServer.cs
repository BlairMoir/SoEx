// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. https://github.com/microsoft/vs-servicehub/blob/main/LICENSE

using System.Diagnostics;

namespace SoEx.Transport.NamedPipe;

/// <summary>
/// Describes an inter-process communication (IPC) server.
/// </summary>
public interface IIpcServer : IAsyncDisposable
{
    /// <summary>
    /// Gets the name of the pipe used to connect to this server.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the <see cref="System.Diagnostics.TraceSource"/> that this server will log to.
    /// </summary>
    TraceSource TraceSource { get; }

    /// <summary>
    /// Gets a task that completes when the server has stopped listening for incoming connections.
    /// </summary>
    Task Completion { get; }
}
