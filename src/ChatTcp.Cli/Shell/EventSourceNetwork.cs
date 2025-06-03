using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal sealed class EventSourceNetwork : IDisposable
{
    private readonly NetworkClient _networkClient = new NetworkClient();
    private readonly Subject<AppEvent> _events = new();

    public IObservable<AppEvent> Events => _events.AsObservable();

    public async Task StartAsync(CancellationToken ct)
    {
        await _networkClient.ConnectAsync("localhost", 8888);
        _events.OnNext(new ConnectedEvent());

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var networkString = await _networkClient.ReadLineAsync(ct);
                if (networkString != null)
                {
                    var message = JsonSerializer.Deserialize<ChatMessage>(networkString);

                    if( message == null)
                        throw new ShellException("Failed serialize text string: " + networkString);

                    _events.OnNext(new NetworkReceiveEvent(message));
                }
            }
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

    public void Dispose()
    {
        _events.OnCompleted();
        _events.Dispose();
        _networkClient.Dispose();
    }
}

