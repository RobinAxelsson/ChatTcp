using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal abstract class AppEvent { }

internal class CharInputEvent : AppEvent
{
    public char Chr { get; }

    public CharInputEvent(char character)
    {
        Chr = character;
    }
}
internal class ConnectedEvent : AppEvent { }
internal class DisconnectedEvent : AppEvent;

internal class NetworkReceiveEvent : AppEvent
{
    public NetworkReceiveEvent(ChatMessage chatMessage)
    {
        ChatMessage = chatMessage;
    }

    public ChatMessage ChatMessage { get; }
}

internal class BackspaceEvent : AppEvent { }

internal class PressEnterEvent : AppEvent { }

internal class PressEscapeEvent : AppEvent { }

internal class ConsoleStartupEvent : AppEvent
{
    public required int WindowWidth { get; init; }
    public required int WindowHeight { get; init; }
}

internal class WindowResizedEvent : AppEvent
{
    public int Width { get; }
    public int Height { get; }

    public WindowResizedEvent(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
