using System.Net.Sockets;
using ChatTcp.Kernel.Resources;

namespace ChatTcp.Kernel;

public readonly record struct TcpClientTask
{
    public Listener Listener { get; }
    public ValueTask<TcpClient> Task { get; }

    public TcpClientTask(Listener listener, ValueTask<TcpClient> task)
    {
        Listener = listener;
        Task = task;
    }
}

public readonly record struct ReceiveMessageTask
{
    public Connection Transport { get; }
    public Task<WirePacketDto> Task { get; }

    public ReceiveMessageTask(Connection transport, Task<WirePacketDto> task)
    {
        Transport = transport;
        Task = task;
    }
}

public readonly record struct SendMessageTask
{
    public Connection Transport { get; }
    public Task Task { get; }

    public SendMessageTask(Connection transport, Task task)
    {
        Transport = transport;
        Task = task;
    }
}

public readonly record struct Entity
{
    private readonly int _id;
    public Entity()
    {
        _id = IdFactory.Create();
    }
    public int Id => _id;
    public static Entity New => new Entity();
}

public static class IdFactory
{
    private static int nextId = 0;
    public static int Create() => Interlocked.Increment(ref nextId);
}

public abstract class WirePacketDto
{
    public string Id { get; init; } = Guid.CreateVersion7().ToString("n");
}

public class ChatMessageDto : WirePacketDto
{
    public ChatMessageDto(string sender, string message)
    {
        Sender = sender;
        Message = message;
    }
    public string Sender { get; init; }
    public string Message { get; init;  }
    public override string ToString() => $"{Sender}:{Message}";
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


