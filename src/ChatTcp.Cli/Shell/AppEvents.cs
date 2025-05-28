using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal abstract class AppEvent { }

internal class CharInputEvent : AppEvent
{
    public char Character { get; }

    public CharInputEvent(char character)
    {
        Character = character;
    }
}

internal class ReceiveMessageEvent : AppEvent
{
    public ReceiveMessageEvent(ChatMessage chatMessage)
    {
        ChatMessage = chatMessage;
    }

    public ChatMessage ChatMessage { get; }
}

internal class BackspaceEvent : AppEvent { }

internal class SendMessageEvent : AppEvent { }

internal class QuitEvent : AppEvent { }
