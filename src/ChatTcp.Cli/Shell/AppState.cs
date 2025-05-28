namespace ChatTcp.Cli.ConsoleUi;
internal class AppState
{
    private int? _cursorIndex;
    public List<ChatMessage> Messages { get; } = new();
    public string InputBuffer { get; set; } = string.Empty;
    public int CursorIndex
    {
        get => _cursorIndex ??= InputBuffer.Length;
        set => _cursorIndex = value;
    }
}
