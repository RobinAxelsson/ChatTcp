using System.Net;
using System.Net.Sockets;
using ChatTcp.Kernel;

namespace ChatTcp.Server;

internal class ChatServer
{
    private const int Port = 8888;
    private readonly IPAddress _ipAddress = IPAddress.Loopback;
    private readonly TcpListener _tcpServer;
    private readonly List<ClientHandler> _clients = new();
    private readonly List<Task> _clientTasks = new();
    public ChatServer()
    {
        _tcpServer = new TcpListener(_ipAddress, Port);
    }

    private async Task BroadcastNewChatMessage(ChatMessageDto message, ClientHandler sender, CancellationToken ct)
    {
        Console.WriteLine($"{message.Sender}: {message.Message}");

        var tasks = new List<Task>();
        foreach (var client in _clients)
        {
            if (client == sender)
                continue;

            ct.ThrowIfCancellationRequested();
            tasks.Add(client.SendChatMessageToClient(message, ct));
        }

        await Task.WhenAll(tasks);
    }

    public async Task Run(CancellationToken ct)
    {
        _tcpServer.Start();
        Console.WriteLine($"Server started on {_ipAddress}:{Port}");

        try
        {
            await AcceptClientsAsync(ct);
        }
        finally
        {
            Stop();
        }
    }

    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var tcpClient = await _tcpServer.AcceptTcpClientAsync(ct);
            var clientHandler = new ClientHandler(tcpClient);
            _clients.Add(clientHandler);

            Console.WriteLine($"Client connected: {tcpClient.Client.RemoteEndPoint}");

            await clientHandler.SendChatMessageToClient(
                new ChatMessageDto("Server", $"Connected to {_ipAddress}:{Port}"), ct);

            // Start listening on this client and track its task
            var clientTask = HandleClientAsync(clientHandler, ct);
            _clientTasks.Add(clientTask);

            // Detach cleanup logic: remove completed tasks to avoid growing list
            _ = clientTask.ContinueWith(t =>
            {
                _clientTasks.Remove(t);

                if (t.Exception != null)
                {
                    Console.WriteLine($"Client handler error: {t.Exception.InnerException}");
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    private async Task HandleClientAsync(ClientHandler clientHandler, CancellationToken ct)
    {
        try
        {
            await clientHandler.Listen(BroadcastNewChatMessage, ct);
        }
        finally
        {
            clientHandler.Dispose();
            _clients.Remove(clientHandler);
            Console.WriteLine($"Client disconnected: {clientHandler.RemoteEndPoint}");
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
