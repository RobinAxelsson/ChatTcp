using System.Reactive.Linq;
using ChatTcp.Cli.Shell;
using ChatTcp.Cli.Shell.Models;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        using var userInputManager = new UserInputManager();
        using var networkManager = new NetworkManager();
        var éventStream = Observable.Merge(userInputManager.Events, networkManager.Events);

        var appEventHandler = new AppEventHandler(cts, networkManager.SendChatMessage);
        éventStream.Subscribe(appEventHandler.Handle);

        var renderer = new Renderer();
        appEventHandler.AppStateStream.Subscribe(renderer.Render);

        var tasks = new[]
        {
            userInputManager.Start(cts.Token),
            networkManager.StartAsync(cts.Token)
        };

        var firstTask = Task.WhenAny(tasks);

        if (firstTask.IsFaulted)
        {
            cts.Cancel();
        }

        await Task.WhenAll(tasks);
    }
}
