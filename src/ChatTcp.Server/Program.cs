// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ChatTcp.Server;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var source = new CancellationTokenSource();
        var chatServer = new ChatServer();
        var serverTask = chatServer.Run(source.Token);

        while (true)
        {
            var input = Console.ReadLine();
            if(input == "cancel")
            {
                source.Cancel();
                break;
            }
        }

        await serverTask;
        Console.WriteLine("Exited gracefully!");
        Console.ReadKey();
    }
}
