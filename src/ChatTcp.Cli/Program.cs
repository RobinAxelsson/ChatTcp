using System.Reactive.Linq;
using ChatTcp.Cli.Shell;
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

        var appStateMutator = new AppStateMutator(cts);
        mergedEvents.Subscribe(appStateMutator.Handle);

        appStateMutator.Events.Subscribe(renderer.Render);

        Console.CancelKeyPress += (_, _) => cts.Cancel();

        var tasks = new[]
        {
            console.Start(cts.Token),
            network.StartAsync(cts.Token)
        };

        Console.CancelKeyPress += (_, _) => cts.Cancel();
        await Task.WhenAll(tasks);
    }
}
