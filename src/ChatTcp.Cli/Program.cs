using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal class Program
{

    private static async Task Main(string[] args)
    {
        var renderer = new Renderer();
        var appState = new AppState();
        using var keyHandler = new KeyHandler();
        using var appLifecycle = new AppLifecycle();

        Seed(appState);

        var appEventObservables = KeyBinder.Bind(keyHandler.KeyStream);

        var appEventHandler = new AppEventHandler(appState, appLifecycle);
        appEventObservables.Subscribe(appEventHandler.Handle);

        var keyHandlerTask = keyHandler.Start(appLifecycle);
        var renderTask = renderer.Start(appState, appLifecycle);

        Task.WaitAll(renderTask, keyHandlerTask);

        Console.Clear();
        Console.WriteLine("Exited gracefully");
    }

    private static void Seed(AppState appState)
    {
        appState.WindowWidth = Console.WindowWidth;
        appState.WindowHeight = Console.WindowHeight;
        appState.Messages.Add(new Message("Bob", "Morning man!", false));
        appState.Messages.Add(new Message("Kalle", "Morning! :)", false));
        appState.InputBuffer = "Morning boys!";
    }
}

