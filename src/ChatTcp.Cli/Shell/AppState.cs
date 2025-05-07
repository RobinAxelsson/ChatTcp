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
    public bool PromptingMode = true;
    public List<Message> Messages { get; } = new();
    public string InputBuffer { get; set; } = string.Empty;
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
}
