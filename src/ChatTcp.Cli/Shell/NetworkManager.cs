using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal sealed class NetworkManager : IDisposable
{
    private readonly Subject<AppEvent> _events = new();
    private readonly Queue<ChatMessage> _outboundChatMessageQueue = new();
    private readonly TcpClient _tcpClient = new();
    private NetworkStream? _networkStream;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public IObservable<AppEvent> Events => _events.AsObservable();
    public async Task StartAsync(CancellationToken ct)
    {
        await ConnectAsync("localhost", 8888);
        _events.OnNext(new ConnectedEvent());

        try
        {
            var receiveTask = ReceiveMessages(ct);
            var sendTask = SendMessages(ct);

            var firstTask = await Task.WhenAny(receiveTask, sendTask);
            await firstTask;

            await Task.WhenAll(receiveTask, sendTask);
        }
        finally
        {
            _events.OnNext(new DisconnectedEvent());
            _events.OnCompleted();
        }

    }

    private async Task ConnectAsync(string host, int port)
    {
        await _tcpClient.ConnectAsync(host, port);
        _networkStream = _tcpClient.GetStream();
        _reader = new StreamReader(_networkStream, Encoding.UTF8);
        _writer = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };
    }

    private async Task<string?> ReadLineAsync(CancellationToken ct)
    {
        if (_reader == null)
        {
            throw new ShellException("reader is null");
        }
        var text = await _reader.ReadLineAsync();
        return text;
    }

    private async Task SendMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_outboundChatMessageQueue.TryDequeue(out var message))
            {
                if (_writer == null)
                    throw new InvalidOperationException("Not connected");

                await _writer.WriteLineAsync(message.Content);
            }
            await Task.Delay(500);
        }
    }

    private async Task ReceiveMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var networkString = await ReadLineAsync(ct);
            if (networkString != null)
            {
                var message = ChatMessage.FromOtherUser("", networkString);
                throw new ArgumentException("test");
                if (message == null)
                    throw new ShellException("Failed serialize text string: " + networkString);

                _events.OnNext(new NetworkReceiveEvent(message));
            }
        }
    }

    public void QueueOutboundChatMessage(ChatMessage chatMessage)
    {
        _outboundChatMessageQueue.Enqueue(chatMessage);
    }

    public void Dispose()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _networkStream?.Dispose();
        _tcpClient.Dispose();

        _events.OnCompleted();
        _events.Dispose();
    }
}

