using System.Net;
using System.Net.Sockets;

namespace ChatTcp.Server;

internal class ChatServer
{
    private const int Port = 8888;
    private readonly IPAddress _ipAddress = IPAddress.Loopback;
    private readonly TcpListener _tcpServer;
    private readonly List<Client> _clients = new();
    public ChatServer()
    {
        _tcpServer = new TcpListener(_ipAddress, Port);
    }

    private async Task OnReceivedMessage(string message, Client client, CancellationToken ct)
    {
        Console.WriteLine(client.Username + ": " + message);

        var tasks = new List<Task>();
        foreach (var c in _clients)
        {
            if (c == client)
                continue;

            ct.ThrowIfCancellationRequested();

            tasks.Add(c.Send(message));
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
                var client = new Client(tcpClient);
                _clients.Add(client);
                Console.WriteLine("Client connected: " + tcpClient.Client.RemoteEndPoint);
                await client.Send("Server: Welcome!");

                _ = client.Listen(OnReceivedMessage, cancellationToken);
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
