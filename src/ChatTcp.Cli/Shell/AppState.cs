namespace ChatTcp.Cli.ConsoleUi;

public record Message(string Sender, string Content);
internal class AppState
{
    public bool PromptingMode = true;
    public List<Message> Messages { get; } = new();
    public string InputBuffer { get; set; } = string.Empty;
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
}
