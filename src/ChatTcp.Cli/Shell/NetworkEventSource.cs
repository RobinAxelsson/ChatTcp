using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal sealed class NetworkEventSource
{
    private readonly NetworkTransport _transport;
    private readonly Subject<AppEvent> _events = new();

    public IObservable<AppEvent> Events => _events.AsObservable();

    public NetworkEventSource(NetworkTransport transport)
    {
        _transport = transport;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _events.OnNext(new ConnectedEvent());

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var networkString = await _transport.ReadLineAsync(ct);
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
    }
}

