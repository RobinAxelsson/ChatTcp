// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ChatTcp.Server;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        var chatServer = new ChatServer();

        var serverTask = chatServer.Run(cts.Token);
        var cancelTask = Task.Run(() =>
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "cancel")
                {
                    cts.Cancel();
                    break;
                }
            }
        });

        try
        {
            await await Task.WhenAny(serverTask, cancelTask);
        }
        catch (Exception)
        {
            cts.Cancel(true);
            throw;
        }
        finally
        {
            await Task.WhenAll(serverTask, cancelTask);
        }

        Console.WriteLine("Exited gracefully!");
        Console.ReadKey();
    }
}
