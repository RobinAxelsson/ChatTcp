using ChatTcp.Cli.ConsoleUi;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //using var chatClient = new ChatClient();
        //var connectTask = chatClient.ConnectAsync(source.Token);

        //while (true)
        //{
        //    var input = Console.ReadLine();
        //    if (input == "cancel")
        //    {
        //        source.Cancel();
        //        break;
        //    }

        //    if (!string.IsNullOrEmpty(input))
        //    {
        //        await chatClient.SendMessage(input);
        //    }
        //}

        //await connectTask;
        var renderer = new Renderer();
        var appState = new AppState();
        using var keyHandler = new KeyHandler();
        using var appLifecycle = new AppLifecycle();
        var appEventHandler = new AppEventHandler(appState, appLifecycle);

        Seed(appState);


        var appEventObservables = KeyBinder.Bind(keyHandler.KeyStream);
        appEventObservables.Subscribe(appEventHandler.Handle);

        var keyHandlerTask = keyHandler.Start(appLifecycle.Token);
        var renderTask = renderer.Start(appState, appLifecycle.Token);

        var firstTask = await Task.WhenAny(renderTask, keyHandlerTask);

        if (firstTask.IsFaulted)
        {
            Console.WriteLine("Task faulted...");
            appLifecycle.RequestShutdown();
        }
        await Task.WhenAll(keyHandlerTask, renderTask); //Exceptions are caught and printed to console
        Console.WriteLine("Exited gracefully");
        Console.ReadKey();
    }

    private static void Seed(AppState appState)
    {
        appState.Messages.Add(new Message("Bob", "Morning man!", false));
        appState.Messages.Add(new Message("Kalle", "Morning! :)", false));
        appState.InputBuffer = "Morning boys!";
    }
}

