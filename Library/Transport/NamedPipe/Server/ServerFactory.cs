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


    private const int ConnectRetryIntervalMs = 50;
    private const int MaxRetryAttemptsForFileNotFoundException = 3;
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

    /// <inheritdoc cref="ConnectAsync(string, ClientOptions, CancellationToken)"/>
    public static Task<Stream> ConnectAsync(string pipeName, CancellationToken cancellationToken) => ConnectAsync(pipeName, default(ClientOptions), cancellationToken);

    /// <summary>
    /// Connects to an IPC pipe that was created with <see cref="Create(Func{Stream, Task}, ServerOptions)"/>.
    /// </summary>
    /// <param name="pipeName">A fully-qualified pipe name, including the path. On Windows the prefixed path should be <c>\\.\pipe\</c>.</param>
    /// <param name="options">Options that can influence how the IPC pipe is connected to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The duplex stream established over the pipe.</returns>
    public static async Task<Stream> ConnectAsync(string pipeName, ClientOptions options, CancellationToken cancellationToken)
    {
        //Requires.NotNull(pipeName, nameof(pipeName));

        PipeOptions fullPipeOptions = StandardPipeOptions;
        PipeOptions pipeOptions = StandardPipeOptions;

        var name = TrimWindowsPrefixForDotNet(pipeName);
        var maxRetries = options.FailFast ? 0 : int.MaxValue;
        PipeStream? pipeStream = null;
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotImplementedException();
                // pipeStream = new AsyncNamedPipeClientStream(".", name, PipeDirection.InOut, pipeOptions);
                // await ((AsyncNamedPipeClientStream)pipeStream).ConnectAsync(maxRetries, ConnectRetryIntervalMs, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                pipeStream = new NamedPipeClientStream(".", name, PipeDirection.InOut, pipeOptions);
                await ConnectWithRetryAsync((NamedPipeClientStream)pipeStream, fullPipeOptions, cancellationToken, maxRetries, withSpinningWait: options.CpuSpinOverFirstChanceExceptions).ConfigureAwait(false);
            }

            return pipeStream;
        }
        catch
        {
            if (pipeStream is not null)
            {
                await pipeStream.DisposeAsync().ConfigureAwait(false);
            }

            throw;
        }
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
    /// Connects to a named pipe without spinning the CPU as <see cref="NamedPipeClientStream.Connect(int)"/> or <see cref="NamedPipeClientStream.ConnectAsync(CancellationToken)"/> would do.
    /// </summary>
    /// <param name="npcs">The named pipe client stream to connect.</param>
    /// <param name="pipeOptions">The pipe options applied to this connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="maxRetries">The maximum number of retries to attempt.</param>
    /// <param name="withSpinningWait">Whether or not the connect should be attempted with a spinning wait.
    /// If the pipe being connected to is known to exist, it is safe to use a spinning wait to avoid potentially throwing exceptions for retries.</param>
    /// <returns>A <see cref="Task"/> that tracks the asynchronous connection attempt.</returns>
    private static async Task ConnectWithRetryAsync(NamedPipeClientStream npcs, PipeOptions pipeOptions, CancellationToken cancellationToken, int maxRetries = int.MaxValue, bool withSpinningWait = false)
    {
        //Requires.NotNull(npcs, nameof(npcs));

        ConcurrentDictionary<string, int> retryExceptions = new ConcurrentDictionary<string, int>();
        int fileNotFoundRetryCount = 0;
        int totalRetries = 0;

        while (true)
        {
            try
            {
                if (withSpinningWait)
                {
                    await npcs.ConnectAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw new NotImplementedException();
                    // Try connecting without wait.
                    // Connecting with anything else will consume CPU causing a spin wait.
                    //await npcs.ConnectAsync((int)NMPWAIT_NOWAIT).ConfigureAwait(false);
                }

                return;
            }
            catch (Exception ex)
            {
                string exceptionType = ex.GetType().ToString();
                retryExceptions.AddOrUpdate(exceptionType, 1, (type, count) => count++);

                if (ex is ObjectDisposedException)
                {
                    // Prefer to throw OperationCanceledException if the caller requested cancellation.
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
                // else if (((ex is IOException && ex.HResult == HRESULT_FROM_WIN32(WIN32_ERROR.ERROR_SEM_TIMEOUT)) || ex is TimeoutException) && totalRetries < maxRetries)
                // {
                // 	// Ignore and retry.
                // 	totalRetries++;
                // }
                else if (ex is FileNotFoundException && fileNotFoundRetryCount < MaxRetryAttemptsForFileNotFoundException && totalRetries < maxRetries)
                {
                    // Ignore and retry.
                    totalRetries++;
                    fileNotFoundRetryCount++;
                }
                else
                {
                    throw;
                }
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(ConnectRetryIntervalMs, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }
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

    /// <summary>
    /// Options that can influence the IPC client.
    /// </summary>
    public record struct ClientOptions
    {
        /// <summary>
        /// Gets a value indicating whether to fail immediately with a <see cref="TimeoutException"/> if the server is not ready to accept the connection.
        /// When this is <see langword="false" />, continuously retry or wait for the server to listen for and respond to connection requests
        /// until it is canceled.
        /// </summary>
        public bool FailFast { get; init; }

        /// <summary>
        /// Gets a value indicating whether to prefer a CPU spinning wait over throwing first chance exceptions as a way to periodically sleep while waiting.
        /// </summary>
        /// <remarks>
        /// This property is only meaningful when <see cref="FailFast"/> is <see langword="false"/>.
        /// </remarks>
        public bool CpuSpinOverFirstChanceExceptions { get; init; }
    }
}
