using System.Net.Sockets;
using System.Text;

namespace ChatTcp.Server;

internal class Client : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _networkStream;
    private readonly StreamWriter _streamWriter;
    private readonly StreamReader _streamReader;

    public string Username { get; set; } = "Unknown";

    public Client(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        Username = _tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        _networkStream = _tcpClient.GetStream();
        _streamWriter = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };
        _streamReader = new StreamReader(_networkStream, Encoding.UTF8);
    }

    public async Task Send(string message)
    {
        await _streamWriter.WriteLineAsync(message);
        await _streamWriter.FlushAsync();
    }

    public async Task Listen(Func<string, Client, CancellationToken, Task> onReceivedMessage, CancellationToken ct)
    {
        string? message;

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000);

            message = await _streamReader.ReadLineAsync();

            if (message == null)
                continue;

            await onReceivedMessage(message, this, ct);
        }
    }

    public void Dispose()
    {
        _streamReader.Dispose();
        _streamWriter.Dispose();
        _networkStream.Dispose();
        _tcpClient.Dispose();
    }
}
