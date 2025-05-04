using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace CliChat.Cli;

internal class RemoteClient : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly Channel<string> _messageChannel;

    public RemoteClient(TcpClient tcpClient, System.Threading.Channels.Channel<string> messageChannel)
    {
        _tcpClient = tcpClient;
        _messageChannel = messageChannel;
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

        var readTask = ReadLoop(streamReader, "client");
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

    private async Task ReadLoop(StreamReader streamReader, string sender)
    {
        string? message;

        while (true)
        {
            Thread.Sleep(1000);

            message = await streamReader.ReadLineAsync();

            if (message == null) continue;

            await _messageChannel.Writer.WriteAsync($"{sender}: {message}");
        }
    }

    public void Dispose()
    {
        _tcpClient.Dispose();
    }
}
