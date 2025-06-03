// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ChatTcp.Cli.Shell
{
    internal abstract class NetworkTransportBase : INetworkSender, IDisposable
    {
        public abstract Task ConnectAsync(string host, int port);
        public abstract Task<string?> ReadLineAsync(CancellationToken ct);
        public abstract Task SendAsync(string message);
        public abstract void Dispose();
    }
}
