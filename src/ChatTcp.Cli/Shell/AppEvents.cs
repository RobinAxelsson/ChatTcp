namespace ChatTcp.Cli;

internal abstract class AppEvent { }

internal class TextInputEvent : AppEvent
{
    public char Character { get; }

    public TextInputEvent(char character)
    {
        Character = character;
    }
}

internal class BackspaceEvent : AppEvent { }

internal class SendMessageEvent : AppEvent { }

internal class QuitEvent : AppEvent { }
