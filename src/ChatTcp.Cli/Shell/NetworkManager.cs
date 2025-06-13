using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal sealed class NetworkManager : IDisposable
{
    private readonly NetworkClient _networkClient = new NetworkClient();
    private readonly Subject<AppEvent> _events = new();
    private readonly Queue<ChatMessage> _outboundChatMessageQueue = new();
    public IObservable<AppEvent> Events => _events.AsObservable();
    public async Task StartAsync(CancellationToken ct)
    {
        await _networkClient.ConnectAsync("localhost", 8888);
        _events.OnNext(new ConnectedEvent());

        try
        {
            var receiveTask = ReceiveMessages(ct);
            var sendTask = SendMessages(ct);
            await Task.WhenAll(receiveTask, sendTask);
        }
        catch (Exception ex)
        {
            _events.OnError(ex);
        }
        finally
        {
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
                await _networkClient.SendAsync(message.Content);
            }
            await Task.Delay(500);
        }
    }

    private async Task ReceiveMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var networkString = await _networkClient.ReadLineAsync(ct);
            if (networkString != null)
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(networkString);

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
        _events.OnCompleted();
        _events.Dispose();
        _networkClient.Dispose();
    }
}

