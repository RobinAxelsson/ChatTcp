
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace ChatTcp.Cli.Networking;

internal class MessageServer : IDisposable
{
    private const int port = 8888;
    private readonly IPAddress _ipAddress = IPAddress.Loopback;
    private TcpListener _tcpServer;
    private List<RemoteClient> _connectedClients = new List<RemoteClient>();
    private bool isStarted = false;
    private Channel<string> _messageChannel;

    public MessageServer()
    {
        _tcpServer = new TcpListener(_ipAddress, port);
        _messageChannel = Channel.CreateUnbounded<string>();
    }

    public void Start()
    {
        if (isStarted)
        {
            throw new ArgumentException("Server is already running");
        }

        _tcpServer.Start();
        Console.WriteLine($"server started on: {_ipAddress}:{port}");

        while (true)
        {
            var connectedClient = new RemoteClient(_tcpServer.AcceptTcpClient(), _messageChannel);
            connectedClient.AddToConsoleChat();
            _connectedClients.Add(connectedClient);
        }
    }

    public void Stop()
    {
        Dispose();
    }
    public void Dispose()
    {
        _connectedClients.ForEach(x => x.Dispose());
        _connectedClients.Clear();
        _tcpServer?.Dispose();
    }
}
