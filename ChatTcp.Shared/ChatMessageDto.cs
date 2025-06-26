namespace ChatTcp.Shared;
public record ChatMessageDto
{
    public ChatMessageDto(string sender, string message)
    {
        Sender = sender;
        Message = message;
    }

    public string Sender { get; }
    public string Message { get; }
}

