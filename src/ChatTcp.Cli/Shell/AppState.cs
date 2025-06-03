using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;
internal record AppState
{
    private int? _cursorIndex;

    public required IReadOnlyCollection<ChatMessage> Messages { get; init; } = [];
    public required string InputBuffer { get; init; } = string.Empty;
    public int CursorIndex
    {
        get => _cursorIndex ??= InputBuffer.Length;
        init => _cursorIndex = value;
    }

#if DEBUG
    public static AppState Debug => new AppState()
    {
        Messages =
        [
            ChatMessage.FromOtherUser("Bob", "Morning man!"),
            ChatMessage.FromOtherUser("Kalle", "Morning! :)")
            ],
        InputBuffer = "Morning boys!"
    };
#endif
}
