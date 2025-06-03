using System.Reactive.Linq;
using ChatTcp.Cli.Shell;
using ChatTcp.Cli.Shell.Models;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        using var console = new EventSourceUser();
        using var network = new EventSourceNetwork();
        var éventStream = Observable.Merge(console.Events, network.Events);

        var appEventHandler = new AppEventHandler(cts);
        éventStream.Subscribe(appEventHandler.Handle);

        var renderer = new Renderer();
        appEventHandler.Events.Subscribe(renderer.Render);

        var tasks = new[]
        {
            console.Start(cts.Token),
            network.StartAsync(cts.Token)
        };

        Console.CancelKeyPress += (_, _) => cts.Cancel();
        await Task.WhenAll(tasks);
    }
}
