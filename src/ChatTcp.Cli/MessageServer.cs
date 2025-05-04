
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CliChat.Cli;

internal class MessageServer : IDisposable
{
    private const int port = 8888;
    private readonly IPAddress _ipAddress = IPAddress.Loopback;
    private TcpListener _tcpServer;
    private List<RemoteClient> _connectedClients = new List<RemoteClient>();
    private bool isStarted = false;

    public MessageServer()
    {
        _tcpServer = new TcpListener(_ipAddress, port);
    }

    public void Start()
    {
        if (isStarted)
        {
            throw new ArgumentException("You started the messagServer twice");
        }

        _tcpServer.Start();
        Console.WriteLine($"server started on: {_ipAddress}:{port}");

        while (true)
        {
            using var tcpClient = _tcpServer.AcceptTcpClient();
            var connectedClient = new RemoteClient(tcpClient);
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

    private class RemoteClient : IDisposable
    {
        private readonly TcpClient _tcpClient;


        public RemoteClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public void AddToConsoleChat()
        {
            var remoteEndpoint = _tcpClient.Client.RemoteEndPoint;
            if (remoteEndpoint == null)
            {
                throw new ArgumentNullException(nameof(remoteEndpoint));
            }

            Console.WriteLine("Client connected: " + remoteEndpoint);

            using var networkStream = _tcpClient.GetStream();
            using var streamWriter = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
            using var streamReader = new StreamReader(networkStream, Encoding.UTF8);



            var readTask = ReadLoop(streamReader, remoteEndpoint);
            var writeTask = WriteLoop(streamWriter);

            Task.WaitAll(readTask, writeTask);
        }

        private static async Task WriteLoop(StreamWriter streamWriter)
        {
            while (true)
            {
                var message = Console.ReadLine();
                await streamWriter.WriteLineAsync(message);
                Thread.Sleep(1000);
            }
        }

        private static async Task ReadLoop(StreamReader streamReader, EndPoint remoteEndpoint)
        {
            string? message;

            while (true)
            {
                Thread.Sleep(1000);

                message = await streamReader.ReadLineAsync();

                if (message == null) continue;

                Console.WriteLine($"{remoteEndpoint}:{message}");
            }
        }

        public void Dispose()
        {
            _tcpClient.Dispose();
        }
    }
}
