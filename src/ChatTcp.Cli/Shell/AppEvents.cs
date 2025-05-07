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

internal class CommandEvent : AppEvent
{
    public string Command { get; }

    public CommandEvent(string command)
    {
        Command = command;
    }
}

internal class QuitEvent : AppEvent { }
