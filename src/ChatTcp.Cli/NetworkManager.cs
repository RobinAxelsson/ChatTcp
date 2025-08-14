//Instructions chatGpt design reasoning:
//You are a .NET senior developer
//You value simple design solutions over complex
//Keep an open mind and ignore previous chats for now
//Give alternatives with pros and cons
//Read the full file and reason about the design, thread safety and readability
//Can you simplify, is it any wrong abstractions, can some things be done differently
//then read it again part by part and write comments as you go
//then assume you could be wrong and iterate again
//Don't add to our chat only comment inside the code
//Don't change the code only comment
//Don't remove these instructions
//Do the instructions

//Evaluate the NetworkManager class


using System.Net;
using System.Net.Sockets;

namespace ChatTcp.Cli;

internal interface IHaveOutbox
{
    void AddToOutbox(WirePacketDto wirePacketDto);
}

internal interface IHaveInbox
{
    void AddToInbox(WirePacketDto wirePacketDto);
}


//TODO use block and normal constructor for this options class
internal record NetworkManagerOptions(List<(IPAddress IPAddress, int Port)> Addresses);

internal sealed class NetworkManager : IHaveOutbox
{
    private readonly NetworkManagerOptions _options;
    private readonly IHaveInbox _chatScreen;
    private readonly EntityStore _entityStore;

    private readonly Dictionary<Task, TaskContext> _taskEvents = new();

    public NetworkManager(NetworkManagerOptions options, IHaveInbox chatScreen, EntityStore entityStore)
    {
        _options = options;
        _chatScreen = chatScreen;
        _entityStore = entityStore;
    }

    public Task StartAsync(CancellationToken ct)
    {
        var allTasks = new List<Task>();
        for (var i = 0; i < _options.Addresses.Count; i++)
        {
            RequestNewHost(_options.Addresses[i].IPAddress, _options.Addresses[i].Port, allTasks);
        }

        return AwaitAllTasks(allTasks, ct);
    }

    private async Task AwaitAllTasks(List<Task> allTasks, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (allTasks.Count == 0)
            {
                try { await Task.Delay(50, ct).ConfigureAwait(false); } catch (OperationCanceledException) { }
                continue;
            }

            var finishedTask = await Task.WhenAny(allTasks).ConfigureAwait(false);
            allTasks.Remove(finishedTask);

            await HandleFinnishedTask(finishedTask, allTasks, ct);
        }

        await Task.WhenAll(allTasks).ConfigureAwait(false);
    }

    private async Task HandleFinnishedTask(Task finishedTask, List<Task> allTasks, CancellationToken token)
    {
        if (_taskEvents.TryGetValue(finishedTask, out var info))
        {
            _taskEvents.Remove(finishedTask);

            switch (info)
            {
                case AcceptClientTaskContext acceptClientTaskContext:
                    await HandleAcceptClientTask(finishedTask, acceptClientTaskContext, allTasks, token).ConfigureAwait(false);
                    break;

                case SendPacketTaskContext sendPacketTaskContext:
                    await HandleSendPacketTask(sendPacketTaskContext, allTasks, token);
                    break;

                case MessageReceivedTaskContext messageReceivedTaskContext:
                    await HandleMessageReceivedTask(messageReceivedTaskContext, allTasks, token);
                    break;

                default:
                    throw new InvalidStateException("Unknown task info.");
            }
        }
        else
        {
            throw new InvalidStateException("Task finishedTask with no associated TaskContext.");
        }
    }

    public void AddToOutbox(WirePacketDto wirePacketDto)
    {
        _taskEvents[new TaskCompletionSource().Task] = new SendPacketTaskContext(wirePacketDto);
        //TODO how to find what connection to send to?
        throw new NotImplementedException();
    }

    public void RequestNewHost(IPAddress ipAdress, int port, List<Task> allTasks)
    {
        var tcpClient = new TcpClient();
        var connectTask = tcpClient.ConnectAsync(ipAdress, port);
        //TODO add taskContext
        throw new NotImplementedException();
    }

    public bool IsConnected(IPAddress ipAdress, int port)
    {
        throw new NotImplementedException();
    }

    private async Task HandleMessageReceivedTask(MessageReceivedTaskContext messageSentTaskContext, List<Task> allTasks, CancellationToken token)
    {
        //TODO switch on each packet type
        throw new NotImplementedException();
    }

    private async Task HandleSendPacketTask(SendPacketTaskContext sendPacketTaskContext, List<Task> allTasks, CancellationToken token)
    {
        //TODO just check if the connection object is healthy or has exception
        throw new NotImplementedException();
    }

    private async Task HandleAcceptClientTask(Task finishedTask, AcceptClientTaskContext context, List<Task> allTasks, CancellationToken token)
    {
        var tcp = await ((Task<TcpClient>)finishedTask).ConfigureAwait(false);
        var next = context.Listener.Socket.AcceptTcpClientAsync(token).AsTask();
        allTasks.Add(next);
        _taskEvents[next] = context;

        var connecetion = new Connection(tcp);
    }
}

public class Listener : IDisposable
{
    public TcpListener Socket { get; }
    public IPAddress Address { get; }
    public int Port { get; }
    public int ReceiversMax { get; } = 3;
    public ListenerState State { get; private set; } = ListenerState.Created;
    public Listener(IPAddress address, int port)
    {
        Address = address;
        Port = port;
        Socket = new TcpListener(address, port);
    }

    public void Start()
    {
        if (State != ListenerState.Created && State != ListenerState.Stopped)
            throw new InvalidOperationException($"Cannot start listener from {State} state.");

        State = ListenerState.Starting;
        try
        {
            Socket.Start();
            State = ListenerState.Listening;
        }
        catch
        {
            State = ListenerState.Faulted;
            throw;
        }
    }

    public void Stop()
    {
        if (State == ListenerState.Listening)
        {
            State = ListenerState.Stopping;
            Socket.Stop();
            State = ListenerState.Stopped;
        }
    }

    public void Dispose()
    {
        Socket.Stop();
        Socket.Dispose();
        State = ListenerState.Disposed;
    }

    public override string ToString()
    {
        return $"|{Address}:{Port}:{State}|";
    }
}

public enum ListenerState
{
    Created,       // Allocated but not started
    Starting,      // Start() called but not yet accepting (optional transitional)
    Listening,     // Accepting connections
    Stopping,      // Stop requested but accepts still completing
    Stopped,       // Fully stopped
    Faulted,       // Hit an unrecoverable error
    Disposed       // Freed resources
}

public class Connection : IDisposable
{
    public TcpClient TcpClient { get; }
    public NetworkStream NetworkStream { get; }

    public Connection(TcpClient client)
    {
        TcpClient = client;
        NetworkStream = client.GetStream();
    }

    public void Dispose()
    {
        NetworkStream?.Dispose();
        TcpClient?.Dispose();
    }

    public override string ToString()
    {
        return $"|{TcpClient?.Client.LocalEndPoint}<->{TcpClient?.Client.RemoteEndPoint}|";
    }
}

public class ChatMessageDto : WirePacketDto
{
    public ChatMessageDto(string sender, string message)
    {
        Sender = sender;
        Message = message;
    }
    public string Sender { get; init; }
    public string Message { get; init; }
    public override string ToString() => $"{Sender}:{Message}";
}

public abstract class WirePacketDto
{
    public string Id { get; init; } = Guid.CreateVersion7().ToString("n");
}

public class JoinChatResponseDto : WirePacketDto
{
    public string ChatId { get; init; }
}

public class JoinChatDto : WirePacketDto
{
    public JoinChatDto(string alias) => Alias = alias;

    public string Alias { get; init; }
}


