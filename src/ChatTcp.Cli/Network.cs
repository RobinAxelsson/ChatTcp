using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ChatTcp.Cli.Shared;

namespace ChatTcp.Cli;

internal sealed class NetworkManager : IHaveOutbox
{
    private readonly NetworkManagerOptions _options;
    private readonly IHaveInbox _chatScreen;

    private readonly Dictionary<Task, TaskContext> _taskEvents = new();

    public NetworkManager(NetworkManagerOptions options, IHaveInbox chatScreen)
    {
        _options = options;
        _chatScreen = chatScreen;
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

            await TaskDispatch(finishedTask, allTasks, ct);
        }

        await Task.WhenAll(allTasks).ConfigureAwait(false);
    }

    private async Task TaskDispatch(Task finishedTask, List<Task> allTasks, CancellationToken token)
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
internal record NetworkManagerOptions(List<(IPAddress IPAddress, int Port)> Addresses);

internal interface IHaveOutbox
{
    void AddToOutbox(WirePacketDto wirePacketDto);
}

internal interface IHaveInbox
{
    void AddToInbox(WirePacketDto wirePacketDto);
}

internal sealed class AcceptClientTaskContext : TaskContext
{
    internal Listener Listener { get; }
    internal AcceptClientTaskContext(Listener listener)
    {
        Listener = listener;
    }
}

internal sealed class SendPacketTaskContext : TaskContext
{
    internal WirePacketDto Packet { get; }
    internal SendPacketTaskContext(WirePacketDto packet)
    {
        Packet = packet;
    }

    public WirePacketDto Result => Packet;
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

internal sealed class ConnectionContext : IDisposable
{
    public Connection Conn { get; }
    public string ConnectionId { get; }

    private readonly Channel<WirePacketDto> _inbound =
        Channel.CreateBounded<WirePacketDto>(new BoundedChannelOptions(1024) { SingleReader = false, SingleWriter = true });

    private readonly Channel<WirePacketDto> _outbound =
        Channel.CreateBounded<WirePacketDto>(new BoundedChannelOptions(1024) { SingleReader = true, SingleWriter = false });

    private readonly CancellationTokenSource _cts = new();
    private Task? _sendLoop;
    private Task? _recvLoop;

    internal ConnectionContext(Connection conn, string connectionId)
    {
        if (conn is null) { throw new InvalidStateException("Connection is null."); }
        if (string.IsNullOrWhiteSpace(connectionId)) { throw new InvalidStateException("ConnectionId required."); }
        Conn = conn;
        ConnectionId = connectionId;
    }

    internal void Start()
    {
        if (_sendLoop is not null || _recvLoop is not null) { throw new InvalidStateException("Already started."); }
        _sendLoop = RunSendAsync(_cts.Token);
        _recvLoop = RunRecvAsync(_cts.Token);
    }

    internal ValueTask EnqueueAsync(WirePacketDto dto, CancellationToken ct = default)
    {
        if (dto is null) { throw new InvalidStateException("Packet is null."); }
        return _outbound.Writer.WriteAsync(dto, ct);
    }

    // Async stream of inbound packets; one consumer at a time per caller.
    internal async IAsyncEnumerable<WirePacketDto> ReadAllAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        while (await _inbound.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (_inbound.Reader.TryRead(out var dto))
            {
                yield return dto;
            }
        }
    }

    private async Task RunSendAsync(CancellationToken ct)
    {
        try
        {
            while (await _outbound.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (_outbound.Reader.TryRead(out var dto))
                {
                    await PacketStream.WritePacketAsync(dto, Conn.NetworkStream, ct).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task RunRecvAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var dto = await PacketStream.ReadPacketAsync(Conn.NetworkStream, ct).ConfigureAwait(false);
                if (!_inbound.Writer.TryWrite(dto))
                {
                    await _inbound.Writer.WriteAsync(dto, ct).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _inbound.Writer.TryComplete();
        }
    }

    internal void Stop()
    {
        _cts.Cancel();
        try { _outbound.Writer.TryComplete(); } catch { }
    }

    public void Dispose()
    {
        Stop();
        try { _sendLoop?.Wait(); } catch { }
        try { _recvLoop?.Wait(); } catch { }
        Conn.Dispose();
        _cts.Dispose();
    }
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

public class PacketStream
{
    private static Dictionary<PayloadType, Type> _typeDict;
    private static Dictionary<Type, PayloadType> _typeDictReverse;

    static PacketStream()
    {
        _typeDict = new() { { PayloadType.ChatMessage, typeof(ChatMessageDto) } };
        _typeDictReverse = new();

        foreach (var kvp in _typeDict)
        {
            _typeDictReverse[kvp.Value] = kvp.Key;
        }
    }

    public static async Task<WirePacketDto> ReadPacketAsync(Stream stream, CancellationToken ct)
    {
        var buffer = new byte[3];

        var read = await stream.ReadAsync(buffer, 0, 3, ct);
        var version = buffer[0];

        if (version != 1)
            throw new InvalidStateException("Only v1 is allowed this is: " + version);

        var payloadType = (PayloadType)buffer[1];

        if (payloadType != PayloadType.ChatMessage)
            throw new InvalidStateException("only chatmessage type is allowed this was: " + (int)payloadType);

        var payloadLength = buffer[2];

        if (payloadLength < 1 || payloadLength > 255)
            throw new InvalidStateException("Payload length exceeds size of byte, payload length: " + payloadLength);

        buffer = new byte[payloadLength];
        await stream.ReadAsync(buffer, 0, payloadLength, ct);

        var sPayload = Encoding.UTF8.GetString(buffer);

        WirePacketDto? packetDto;
        try
        {
            packetDto = JsonSerializer.Deserialize(sPayload, _typeDict[payloadType]) as WirePacketDto;
        }
        catch (JsonException ex)
        {
            throw new InvalidStateException($"Json packet payload could not be deserialized. PayloadType: '{payloadType}' payload: {sPayload}", ex);
        }

        if (packetDto == null)
            throw new InvalidStateException("The payload: " + sPayload + " could not be deserialized to type: " + _typeDict[payloadType].Name);

        return packetDto;
    }

    public static async Task WritePacketAsync

        <T>(T packetDto, Stream stream, CancellationToken ct) where T : WirePacketDto
    {
        const byte version = 1;

        byte payloadType = (byte)_typeDictReverse[typeof(T)];

        if (payloadType != (byte)PayloadType.ChatMessage)
            throw new InvalidStateException("only chatmessage type is allowed this was: " + (int)payloadType);

        var payload = JsonSerializer.Serialize(packetDto);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        if (payloadBytes.Length > 255)
            throw new InvalidStateException("Payload is to long, allowed bytes are 255 this was: " + payloadBytes.Length);

        byte bytesLength = (byte)payloadBytes.Length;

        var buffer = new byte[bytesLength + 3];
        buffer[0] = version;
        buffer[1] = payloadType;
        buffer[2] = bytesLength;

        for (int i = 0; i < payloadBytes.Length; i++)
        {
            buffer[i + 3] = payloadBytes[i];
        }

        await stream.WriteAsync(buffer, ct);
    }
}

internal enum PayloadType
{
    Unknown = 0,
    ChatMessage = 1
}



internal sealed class MessageReceivedTaskContext : TaskContext
{
    internal WirePacketDto Packet { get; }
    internal MessageReceivedTaskContext(WirePacketDto packet)
    {
        Packet = packet;
    }

    public WirePacketDto Result => Packet;
}

internal abstract class TaskContext
{
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
