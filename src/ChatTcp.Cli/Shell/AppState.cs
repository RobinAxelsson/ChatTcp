namespace ChatTcp.Cli.ConsoleUi;
public record Message
{
    public Message(string sender, string content, bool currentUser)
    {
        Sender = sender;
        IsCurrentUser = currentUser;
        Content = content;
    }

    public string Sender { get; }
    public bool IsCurrentUser { get; }
    public string Content { get; }
}
internal class AppState
{
    private int? _cursorIndex;

    public List<Message> Messages { get; } = new();
    public string InputBuffer { get; set; } = string.Empty;
    public int CursorIndex
    {
        get => _cursorIndex ??= InputBuffer.Length;
        set => _cursorIndex = value;
    }
}
