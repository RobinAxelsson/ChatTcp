using System.Net.Sockets;
using ChatTcp.Kernel.Resources;

namespace ChatTcp.Kernel;

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

internal sealed class InvalidStateException : Exception
{
    internal InvalidStateException(string message) : base(message)
    {
    }

    internal InvalidStateException(string message, Exception inner) : base(message, inner)
    {
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

public readonly record struct TcpClientTask
{
    public KernelListener Listener { get; }
    public ValueTask<TcpClient> Task { get; }

    public TcpClientTask(KernelListener listener, ValueTask<TcpClient> task)
    {
        Listener = listener;
        Task = task;
    }
}

public readonly record struct ReceiveMessageTask
{
    public KernelConnection Transport { get; }
    public Task<WirePacketDto> Task { get; }

    public ReceiveMessageTask(KernelConnection transport, Task<WirePacketDto> task)
    {
        Transport = transport;
        Task = task;
    }
}

public readonly record struct SendMessageTask
{
    public KernelConnection Transport { get; }
    public Task Task { get; }

    public SendMessageTask(KernelConnection transport, Task task)
    {
        Transport = transport;
        Task = task;
    }
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


