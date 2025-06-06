namespace ChatTcp.Cli.Shell.Models;

public record ChatMessage
{
    public ChatMessage(string senderName, string content, SenderType senderType)
    {
        SenderName = senderName;
        SenderType = senderType;
        Content = content;
    }

    public static ChatMessage FromCurrentUser(string content)
    {
        return new ChatMessage("", content, SenderType.CurrentUser);
    }

    public static ChatMessage FromServer(string content)
    {
        return new ChatMessage("Server", content, SenderType.Server);
    }

    public static ChatMessage FromOtherUser(string name, string content)
    {
        return new ChatMessage(name, content, SenderType.OtherUser);
    }

    public string SenderName { get; }
    public SenderType SenderType { get; }
    public string Content { get; }
}

public enum SenderType
{
    CurrentUser,
    OtherUser,
    Server
}
