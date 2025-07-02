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

        networkManager.OnPacketReceivedFromServer = consoleChat.ReceiveServerPacket;
        consoleChat.OnCurrentUserMessageSubmitted = networkManager.SendChatMessageToServer;

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
