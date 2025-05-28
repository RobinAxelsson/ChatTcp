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
        using var cts = new CancellationTokenSource();
        var appEventHandler = new AppEventHandler(appState, cts);

        Seed(appState);

        var keyPressEvents = KeyBinder.BindKeysToEvents(keyHandler.KeyStream);
        keyPressEvents.Subscribe(appEventHandler.Handle);

        var keyHandlerTask = keyHandler.Start(cts.Token);
        var renderTask = renderer.Start(appState, cts.Token);
        var firstTask = await Task.WhenAny(renderTask, keyHandlerTask);

        if (firstTask.IsFaulted)
        {
            Console.WriteLine("Task faulted...");
            cts.Cancel();
        }
        await Task.WhenAll(keyHandlerTask, renderTask); //Exceptions are caught and printed to console
        Console.WriteLine("Exited gracefully");
        Console.ReadKey();
    }

    private static void Seed(AppState appState)
    {
        appState.Messages.Add(ChatMessage.FromOtherUser("Bob", "Morning man!"));
        appState.Messages.Add(ChatMessage.FromOtherUser("Kalle", "Morning! :)"));
        appState.InputBuffer = "Morning boys!";
    }
}

