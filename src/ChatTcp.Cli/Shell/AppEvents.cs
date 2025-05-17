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

internal class BackspaceEvent : AppEvent { }

internal class SendMessageEvent : AppEvent { }

internal class QuitEvent : AppEvent { }
