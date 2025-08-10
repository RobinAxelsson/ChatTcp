namespace ChatTcp.Kernel;

public readonly record struct Entity
{
    private static int nextId = 1;
    private readonly int _id;
    public Entity()
    {
        _id = Interlocked.Increment(ref nextId);
    }
    public int Id => _id;
    public static Entity New => new Entity();
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


