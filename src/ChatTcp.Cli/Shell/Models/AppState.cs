
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

    public static AppState Debug => new AppState()
    {
        Messages = [
        ChatMessage.FromServer("You are connected"),
            ChatMessage.FromServer("Have fun"),
            ChatMessage.FromOtherUser("Karl", "Hello!"),
            ChatMessage.FromCurrentUser("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus interdum ex sit amet augue maximus, a rhoncus magna cursus. Pellentesque et turpis sit amet quam sagittis accumsan. In id sem ornare, ornare ligula et, iaculis eros. Aenean dignissim elit non magna lobortis, at iaculis ex lacinia. Sed a diam nec nisl mollis dignissim sed ut purus. Nunc sit amet ipsum suscipit, consectetur mauris vel, posuere augue. Sed at est non nulla tincidunt vehicula. Nullam molestie gravida arcu. Vivamus pellentesque neque at purus consequat, sed commodo magna maximus."),
            ChatMessage.FromOtherUser("Liza", "Okey..."),
            ],
            CursorIndex = -1,
            InputBuffer = "Hello world, we are one",
            WindowHeight = Console.WindowHeight,
            WindowWidth = Console.WindowWidth,
        };
}
