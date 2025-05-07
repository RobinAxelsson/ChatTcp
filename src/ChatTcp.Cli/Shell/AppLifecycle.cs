// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ChatTcp.Cli;

public class AppLifecycle : IDisposable
{
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public CancellationToken Token => _cts.Token;

    public void RequestShutdown()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}
