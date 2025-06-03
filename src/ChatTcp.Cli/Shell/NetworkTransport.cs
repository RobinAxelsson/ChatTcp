using System.Net.Sockets;
using System.Text;

namespace ChatTcp.Cli.Shell;

internal sealed class NetworkTransport : NetworkTransportBase, IDisposable
{
    private readonly TcpClient _tcpClient = new();
    private NetworkStream? _networkStream;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public override async Task ConnectAsync(string host, int port)
    {
        await _tcpClient.ConnectAsync(host, port);
        _networkStream = _tcpClient.GetStream();
        _reader = new StreamReader(_networkStream, Encoding.UTF8);
        _writer = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };
    }

    public override async Task<string?> ReadLineAsync(CancellationToken ct)
    {
        return _reader != null ? await _reader.ReadLineAsync() : null;
    }

    public override Task SendAsync(string message)
    {
        if (_writer == null) throw new InvalidOperationException("Not connected");
        return _writer.WriteLineAsync(message);
    }

    public override void Dispose()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _networkStream?.Dispose();
        _tcpClient.Dispose();
    }
}
