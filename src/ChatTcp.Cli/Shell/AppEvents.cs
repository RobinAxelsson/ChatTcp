using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal abstract class AppEvent { }

internal class CharInputEvent : AppEvent
{
    public char Character { get; }

    public CharInputEvent(char character)
    {
        Character = character;
    }
}
internal class ConnectedEvent : AppEvent;
internal class DisconnectedEvent : AppEvent;

internal class MessageReceivedEvent : AppEvent
{
    public MessageReceivedEvent(ChatMessage chatMessage)
    {
        ChatMessage = chatMessage;
    }

    public ChatMessage ChatMessage { get; }
}

internal class BackspaceEvent : AppEvent { }

internal class SendMessageEvent : AppEvent { }

internal class QuitEvent : AppEvent { }

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
