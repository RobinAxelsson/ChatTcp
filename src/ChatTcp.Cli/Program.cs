using ChatTcp.Cli.Shell;

namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        using var networkManager = new NetworkManager();
        var consoleChat = new ConsoleChat();

        consoleChat.OnCurrentUserMessageSubmitted = networkManager.QueueOutboundChatMessage;

        networkManager.Events.Subscribe(e =>
        {
            if (e is NetworkReceiveEvent)
            {
                consoleChat.AddMessage(((NetworkReceiveEvent)e).ChatMessage.Content);
            }

            if(e is DisconnectedEvent)
            {
                consoleChat.AddMessage("Server disconnected");
            }

            if (e is ConnectedEvent)
            {
                consoleChat.AddMessage("Server connected");
            }
        });

        Console.Clear();
        Console.WriteLine("ChatTcp running");

        string? alias = null;
        while(alias == null || string.IsNullOrWhiteSpace(alias)){
            Console.Write("Enter alias: ");
            alias = Console.ReadLine()?.Trim();
        }

        string? host = null;
        while (host == null || string.IsNullOrWhiteSpace(host))
        {
            Console.Write("Enter host: ");
            host = Console.ReadLine()?.Trim();
        }

        int port = 0;
        string? sPort = null;
        while (sPort == null || !int.TryParse(sPort, out port))
        {
            Console.Write("Enter port: ");
            sPort = Console.ReadLine()?.Trim();
        }

        Console.Clear();

        var serverTask = networkManager.StartAsync(cts, host, port);
        var consoleTask = consoleChat.Start(cts.Token);

        var firstTask = Task.WhenAny(serverTask, consoleTask);
        if (firstTask.IsFaulted)
        {
            cts.Cancel();
            throw firstTask.Exception;
        }

        await Task.WhenAll(serverTask, consoleTask);

    }
}
