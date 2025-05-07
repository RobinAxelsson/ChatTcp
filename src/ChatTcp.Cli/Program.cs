using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal class Program
{

    private static async Task Main(string[] args)
    {
        var renderer = new Renderer();
        var appState = new AppState();
        using var keyHandler = new KeyHandler();

        appState.WindowWidth = Console.WindowWidth;
        appState.WindowHeight = Console.WindowHeight;
        appState.Messages.Add(new Message("Bob", "Morning man!", false));
        appState.Messages.Add(new Message("Kalle", "Morning! :)", false));
        appState.InputBuffer = "Morning boys!";

        var eventStream = KeyBinder.Bind(keyHandler.KeyStream);
        var appStateHandler = new AppStateHandler(appState);
        eventStream.Subscribe(appStateHandler.Handle);

        var task = keyHandler.Start();
        _ = renderer.Start(appState);

        task.Wait();
    }
}

