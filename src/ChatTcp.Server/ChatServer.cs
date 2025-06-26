using System.Net;
using System.Net.Sockets;

namespace ChatTcp.Server;

internal class ChatServer
{
    private const int Port = 8888;
    private readonly IPAddress _ipAddress = IPAddress.Loopback;
    private readonly TcpListener _tcpServer;
    private readonly List<ClientHandler> _clients = new();
    private readonly List<Task> _listenTasks = new();
    public ChatServer()
    {
        _tcpServer = new TcpListener(_ipAddress, Port);
    }

    private async Task OnReceivedMessage(ChatMessageDto message, ClientHandler client, CancellationToken ct)
    {
        Console.WriteLine(message.Sender + ": " + message.Message);

        var tasks = new List<Task>();
        foreach (var c in _clients)
        {
            if (c == client)
                continue;

            ct.ThrowIfCancellationRequested();

            tasks.Add(c.SendChatMessageToClient(message));
        }

        await Task.WhenAll(tasks);
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        _tcpServer.Start();

        Console.WriteLine($"Server started on: {_ipAddress}:{Port}");

        try
        {
            while (true)
            {
                var tcpClient = await _tcpServer.AcceptTcpClientAsync(cancellationToken);

                 var client = new ClientHandler(tcpClient);
                _clients.Add(client);
                Console.WriteLine("ClientHandler connected: " + tcpClient.Client.RemoteEndPoint);
                await client.SendChatMessageToClient(new ChatMessageDto("Server " + _ipAddress, "Welcome to chat"));

                 client.Listen(OnReceivedMessage, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancel accept tcp clients...");
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Stop();
        }

    }

    public void Stop()
    {
        Console.WriteLine("Closing server...");
        foreach (var client in _clients)
        {
            client?.Dispose();
        }
        _clients.Clear();
        _tcpServer.Stop();

        Console.WriteLine("Server closed.");
    }
}
