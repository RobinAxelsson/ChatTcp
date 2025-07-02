// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ChatTcp.Server;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        var chatServer = new ChatServer();
        var chatTask = chatServer.Run(cts.Token);
        try
        {
            await chatTask;
        }
        finally
        {
            cts.Cancel();
            await chatTask;
            Console.WriteLine("Exited gracefully!");
        }

    }
}
