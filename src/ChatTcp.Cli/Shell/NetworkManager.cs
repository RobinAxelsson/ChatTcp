using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ChatTcp.Kernel;

namespace ChatTcp.Cli.Shell;

internal sealed class NetworkManager : IDisposable
{
    private readonly Subject<AppEvent> _events = new();
    private readonly ConcurrentQueue<ChatMessageDto> _outboundChatMessageQueue = new();
    private readonly TcpClient _tcpClient = new();
    private NetworkStream? _networkStream;

    public IObservable<AppEvent> Events => _events.AsObservable();

    public void QueueOutboundChatMessage(ChatMessageDto chatMessage)
    {
        _outboundChatMessageQueue.Enqueue(chatMessage);
    }

    public async Task StartAsync(CancellationTokenSource cts, string host = "localhost", int port = 8888)
    {
        await _tcpClient.ConnectAsync(host, port);
        _networkStream = _tcpClient.GetStream();
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
                if(_networkStream == null)
                {
                    throw new ShellException(nameof(_networkStream) + " is null");
                }

                await PacketStream.WritePacketAsync(message, _networkStream, ct);
            }
            await Task.Delay(500);
        }

        Console.WriteLine(nameof(SendMessages) + " exited gracefully");
    }

    private async Task ReceiveMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_networkStream == null)
            {
                throw new ShellException(nameof(_networkStream) + " is null");
            }

            var message = await PacketStream.ReadPacketAsync(_networkStream, ct);
            _events.OnNext(new NetworkReceiveEvent(message));
        }

        Console.WriteLine(nameof(ReceiveMessages) + " exited gracefully");
    }

    public void Dispose()
    {
        _networkStream?.Dispose();
        _tcpClient.Dispose();

        _events.OnCompleted();
        _events.Dispose();
    }
}

