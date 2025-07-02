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

        try
        {
            await await Task.WhenAny(serverTask, consoleTask);
        }
        finally
        {
            cts.Cancel();
            await Task.WhenAll(serverTask, consoleTask);
        }
    }
}
