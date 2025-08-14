using System.Net;
using System.Net.Sockets;

namespace ChatTcp.Cli;
internal interface IHaveOutbox
{
    public Task AddToOutbox(WirePacketDto wirePacketDto);
}

internal interface IHaveInbox
{
    public void AddToInbox(WirePacketDto wirePacketDto);
}

internal class Event
{

}

internal class SendPacketEvent
{

}

internal class NetworkManager : IHaveOutbox
{
    internal record NetworkManagerOptions(List<(IPAddress IPAddress, int Port)> Addresses);

    private readonly NetworkManagerOptions _options;
    private readonly IHaveInbox _chatScreen;
    private readonly EntityStore _entityStore;
    private List<Task<TcpClient>> _getTcpClientTasks = new();
    private Dictionary<Task, Event> _taskEvents = new();

    public NetworkManager(NetworkManagerOptions options, IHaveInbox chatScreen, EntityStore entityStore)
    {
        _options = options;
        _chatScreen = chatScreen;
        _entityStore = entityStore;
    }

    public Task AddToOutbox(WirePacketDto wirePacketDto)
    {
        throw new NotImplementedException();
    }

    public Task StartAsync(CancellationToken ct)
    {
        var listeners = new List<Listener>();

        foreach (var address in _options.Addresses)
        {
            var listener = new Listener(address.IPAddress, address.Port);
            listeners.Add(listener);
            listener.Start();
        }

        _getTcpClientTasks = new List<Task<TcpClient>>();
        foreach (var listener in listeners)
        {
            var tcpClientTask = listener.Socket.AcceptTcpClientAsync();
            _getTcpClientTasks.Add(tcpClientTask);
        }
    }

    private async Task ManageTasks(CancellationToken ct)
    {
        var tasks = new List<Task>();
        tasks.AddRange(_getTcpClientTasks);
        tasks.AddRange(_sendMessageTasks);
        tasks.AddRange(_receiveTasks);
        Dictionary<Task, Event> _taskEvents = new();

        while (ct.IsCancellationRequested)
        {
            var finnishedTask = await Task.WhenAny(tasks);

            if(_taskEvents.TryGetValue(finnishedTask, out var eventObj))
            {
                HandleTask(eventObj, finnishedTask);
            }
        }

        await Task.WhenAll(tasks);
    }

    private void HandleTask(Event eventObj, Task finnishedTask) => throw new NotImplementedException();
}
