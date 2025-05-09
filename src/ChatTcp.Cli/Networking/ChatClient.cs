
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatTcp.Cli.Networking;

internal class ChatClient : IDisposable
{
    private NetworkStream? _networkStream;
    private StreamWriter? _streamWriter;
    private StreamReader? _streamReader;
    private TcpClient? _tcpClient;

    public Action<string> OnMessageReceived { get; set; } = default!;

    public async Task ConnectAsync(CancellationToken ct)
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(IPAddress.Loopback, 8888);
        Console.WriteLine("Connected to server");

        _networkStream = _tcpClient.GetStream();
        _streamWriter = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };
        _streamReader = new StreamReader(_networkStream, Encoding.UTF8);

        while (!ct.IsCancellationRequested)
        {
            var message = await _streamReader.ReadLineAsync();
            if (message != null)
            {
                Console.WriteLine(message);
            }
        }
    }

    public async Task SendMessage(string message)
    {
        if(_streamWriter == null)
        {
            Console.WriteLine("Streamwriter is null");
            Disconnect();
            Environment.Exit(1);
        }

        await _streamWriter.WriteLineAsync(message);
    }

    public void Disconnect()
    {
        _streamReader?.Dispose();
        _streamWriter?.Dispose();
        _networkStream?.Dispose();
        _tcpClient?.Dispose();
    }

    public void Dispose()
    {
        Disconnect();
    }
}
