namespace ChatTcp.Kernel;

public class ChatMessageDto : ChatPacketDto
{
    public ChatMessageDto(string sender, string message)
    {
        Sender = sender;
        Message = message;
    }
    public string Sender { get; init; }
    public string Message { get; init;  }
}

public abstract class ChatPacketDto
{
    
}
