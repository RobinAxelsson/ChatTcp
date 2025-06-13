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

        var serverTask = networkManager.StartAsync(cts.Token);

        consoleChat.Activate(cts.Token);

        await serverTask;
    }
}
