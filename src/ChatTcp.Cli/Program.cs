using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using ChatTcp.Cli.Networking;
using ChatTcp.Cli.Shell;
using ChatTcp.Cli.Shell.Components;
using ChatTcp.Cli.Shell.Models;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        using var transport = new NetworkTransport();
        await transport.ConnectAsync("localhost", 8888);

        using var console = new ConsoleEventSource();
        var network = new NetworkEventSource(transport);
        var renderer = new Renderer();

        var mergedEvents = Observable.Merge(console.Events, network.Events);

        mergedEvents.Subscribe(renderer.Handle);

        var renderer = new Renderer(stateStream);
        var client = new NetworkClient(transport, stateStream);

        Console.CancelKeyPress += (_, _) => cts.Cancel();

        var tasks = new[]
        {
            console.Start(cts.Token),
            network.ConnectAsync(cts.Token)
        };

        Console.CancelKeyPress += (_, _) => cts.Cancel();
        await Task.WhenAll(tasks);
    }

    private static void Seed(AppState appState)
    {
        appState.Messages.Add(ChatMessage.FromOtherUser("Bob", "Morning man!"));
        appState.Messages.Add(ChatMessage.FromOtherUser("Kalle", "Morning! :)"));
        appState.InputBuffer = "Morning boys!";
    }
}
