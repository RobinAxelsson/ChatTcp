using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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

    public void QueueOutboundChatMessage(ChatMessage chatMessage)
    {
        _outboundChatMessageQueue.Enqueue(chatMessage);
    }

    public async Task StartAsync(CancellationTokenSource cts, string host = "localhost", int port = 8888)
    {
        await _tcpClient.ConnectAsync(host, port);
        _networkStream = _tcpClient.GetStream();
        _reader = new StreamReader(_networkStream, Encoding.UTF8);
        _writer = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };

        _events.OnNext(new ConnectedEvent());

        var receiveTask = ReceiveMessages(cts.Token);
        var sendTask = SendMessages(cts.Token);

        try
        {
            //If any task throws we want to fail fast
            await await Task.WhenAny(receiveTask, sendTask);
        }
        catch
        {
            cts.Cancel();
            throw;
        }
        finally
        {
            await Task.WhenAll(receiveTask, sendTask);
            _events.OnNext(new DisconnectedEvent());
            _events.OnCompleted();
        }
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

        Console.WriteLine(nameof(SendMessages) + " exited gracefully");
    }

    private async Task ReceiveMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_reader == null)
            {
                throw new ShellException("reader is null");
            }
            var networkString = await _reader.ReadLineAsync();

            if (networkString != null)
            {
                var message = ChatMessage.FromOtherUser("", networkString);
                if (message == null)
                    throw new ShellException("Failed serialize text string: " + networkString);

                _events.OnNext(new NetworkReceiveEvent(message));
            }
        }

        Console.WriteLine(nameof(ReceiveMessages) + " exited gracefully");
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

