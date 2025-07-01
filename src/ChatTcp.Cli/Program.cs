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

        Console.Clear();
        Console.WriteLine("ChatTcp running");

        string? alias = null;
        while(alias == null || string.IsNullOrWhiteSpace(alias)){
            Console.Write("Enter alias: ");
            alias = Console.ReadLine()?.Trim();
        }

        Console.Clear();

        consoleChat.OnCurrentUserMessageSubmitted = message => networkManager.SendChatMessageToServer(new ChatMessageDto(alias, message));
        networkManager.OnPacketReceivedFromServer = consoleChat.ReceiveServerPacket;

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
