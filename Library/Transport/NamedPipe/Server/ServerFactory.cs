// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;


namespace SoEx.Transport.NamedPipe;

/// <summary>
/// A static class for creating named pipe servers.
/// </summary>
public static class ServerFactory
{
    /// <summary>
    /// The standard pipe options to use.
    /// </summary>
    internal const PipeOptions StandardPipeOptions = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    private static readonly string PipePrefix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\\.\pipe" : Path.GetTempPath();

    /// <summary>
    /// Creates an IPC server.
    /// </summary>
    /// <param name="onConnectedCallback">
    /// Callback function to be run whenever a client connects to the server. This may be called concurrently if multiple clients connect.
    /// The delegate may choose to return right away while still using the <see cref="Stream"/> or to complete only after finishing communication with the client.
    /// </param>
    /// <param name="options">IPC server options.</param>
    /// <returns>
    /// The server, which includes a means to obtain its pipe name and monitor for completion.
    /// </returns>
    public static IIpcServer Create(Func<Stream, Task> onConnectedCallback, ServerOptions options = default)
    {
        //Requires.NotNull(onConnectedCallback, nameof(onConnectedCallback));

        return CreateCore(onConnectedCallback, options);
    }

    /// <summary>
    /// Prepends the OS-specific prefix to a simple pipe name.
    /// </summary>
    /// <param name="leafPipeName">The simple pipe name. This should <em>not</em> include a path.</param>
    /// <returns>The fully-qualified, OS-specific pipe name.</returns>
    public static string PrependPipePrefix(string leafPipeName) => Path.Combine(PipePrefix, leafPipeName);

    /// <summary>
    /// Removes the prefix from a pipe name if it is fully-qualified and on Windows where the prefix should <em>not</em> be used in the .NET APIs.
    /// </summary>
    /// <param name="fullyQualifiedPipeName">The fully-qualified path.</param>
    /// <returns>The pipe name to use with .NET APIs. This <em>may</em> still be fully-qualified.</returns>
    internal static string TrimWindowsPrefixForDotNet(string fullyQualifiedPipeName)
    {
        const string WindowsPipePrefix = @"\\.\pipe\";
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && fullyQualifiedPipeName.StartsWith(WindowsPipePrefix, StringComparison.OrdinalIgnoreCase)
            ? fullyQualifiedPipeName.Substring(WindowsPipePrefix.Length)
            : fullyQualifiedPipeName;
    }

    private static IpcServer CreateCore(Func<Stream, Task> onConnectedCallback, ServerOptions options)
    {
        return new IpcServer(options with { PipeOptions = StandardPipeOptions }, onConnectedCallback);
    }

    /// <summary>
    /// Options that can influence the IPC server.
    /// </summary>
    public record struct ServerOptions
    {
        /// <summary>
        /// Gets the fully-qualified name of the pipe to accept connections to.
        /// </summary>
        /// <remarks>
        /// This should include the <c>\\.\pipe\</c> prefix on Windows, or the absolute path to a file to be created on linux/mac.
        /// </remarks>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the means of logging regarding connection attempts.
        /// </summary>
        public TraceSource? TraceSource { get; init; }

        /// <summary>
        /// Gets a value indicating whether to serve more than one incoming client.
        /// </summary>
        public bool AllowMultipleClients { get; init; }

        /// <summary>
        /// Gets the options to use on the named pipes.
        /// </summary>
        internal PipeOptions PipeOptions { get; init; }
    }
}
