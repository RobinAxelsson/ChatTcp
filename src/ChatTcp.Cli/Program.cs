using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal class Program
{

    private static void Main(string[] args)
    {
        var renderer = new Renderer();
        var appState = new AppState();
        appState.WindowWidth = Console.WindowWidth;
        appState.WindowHeight = Console.WindowHeight;
        appState.Messages.Add(new Message("Bob", "Morning man!"));
        appState.Messages.Add(new Message("Kalle", "Morning! :)"));
        appState.InputBuffer = "Morning boys!";

        renderer.RenderApp(appState);
        Console.Read();
    }
}

