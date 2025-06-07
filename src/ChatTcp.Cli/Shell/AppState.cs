
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;
internal record AppState
{
    private int? _cursorIndex;
    public int WindowWidth { get; init; } = default;
    public int WindowHeight { get; init; } = default;
    public IReadOnlyCollection<ChatMessage> Messages { get; init; } = [];
    public string InputBuffer { get; init; } = string.Empty;
    public int CursorIndex
    {
        get => _cursorIndex ??= InputBuffer.Length;
        init => _cursorIndex = value;
    }
}
