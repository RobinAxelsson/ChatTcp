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

        var task = TaskExtensions.RunWithCrashOnException(keyHandler.Start);
        _ = TaskExtensions.RunWithCrashOnException(() => renderer.Start(appState));

        task.Wait();
    }
}

internal static class TaskExtensions
{
    public static async Task RunWithCrashOnException(Func<CancellationToken, Task> taskFunc, CancellationToken cancellationToken = default)
    {
        try
        {
            await taskFunc(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Task canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Critical error in task:");
            Console.WriteLine(ex);
            Environment.Exit(1); // Crash the app, like a main thread exception
        }
    }

    public static async Task RunWithCrashOnException(Func<Task> taskFunc)
    {
        await RunWithCrashOnException(_ => taskFunc(), CancellationToken.None);
    }
}

