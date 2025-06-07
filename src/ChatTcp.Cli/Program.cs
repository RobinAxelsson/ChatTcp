using System.Reactive.Linq;
using ChatTcp.Cli.Shell;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        using var userInputManager = new ConsoleInputManager();
        using var networkManager = new NetworkManager();
        var éventStream = Observable.Merge(userInputManager.Events, networkManager.Events);

        var appEventHandler = new AppEventHandler(cts);
        éventStream.Subscribe(appEventHandler.Handle);

        var consoleOutputManager = new ConsoleOutputManager();
        appEventHandler.AppStateStream.Subscribe(consoleOutputManager.Handle);
        appEventHandler.ChatMessageStream.Subscribe(networkManager.QueueChatMessage);

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
