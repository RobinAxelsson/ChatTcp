using ChatTcp.Cli.Shell;
using ChatTcp.Kernel;

namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        using var networkManager = new NetworkManager();
        var consoleChat = new ConsoleChat();


        networkManager.Events.Subscribe(e =>
        {
            if (e is NetworkReceiveEvent)
            {
                consoleChat.AddMessage((ChatMessageDto)((NetworkReceiveEvent)e).Packet);
            }

            if(e is DisconnectedEvent)
            {
                consoleChat.AddMessage(new ChatMessageDto("Logger", "disconnected"));
            }

            if (e is ConnectedEvent)
            {
                consoleChat.AddMessage(new ChatMessageDto("Logger", "connected"));
            }
        });

        Console.Clear();
        Console.WriteLine("ChatTcp running");

        string? alias = null;
        while(alias == null || string.IsNullOrWhiteSpace(alias)){
            Console.Write("Enter alias: ");
            alias = Console.ReadLine()?.Trim();
        }

        //string? host = null;
        //while (host == null || string.IsNullOrWhiteSpace(host))
        //{
        //    Console.Write("Enter host: ");
        //    host = Console.ReadLine()?.Trim();
        //}

        //int port = 0;
        //string? sPort = null;
        //while (sPort == null || !int.TryParse(sPort, out port))
        //{
        //    Console.Write("Enter port: ");
        //    sPort = Console.ReadLine()?.Trim();
        //}

        Console.Clear();

        consoleChat.OnCurrentUserMessageSubmitted = message => networkManager.QueueOutboundChatMessage(new ChatMessageDto(alias, message));

        var serverTask = networkManager.StartAsync(cts);
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
