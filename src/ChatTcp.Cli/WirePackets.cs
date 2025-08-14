namespace ChatTcp.Cli;

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


