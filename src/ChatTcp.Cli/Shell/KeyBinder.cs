using System.Reactive.Linq;

namespace ChatTcp.Cli;

internal class KeyBinder
{
    public static IObservable<AppEvent?> Bind(IObservable<ConsoleKeyInfo> keyStream)
    {
        return keyStream.Select(key =>
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                return (AppEvent) new BackspaceEvent();
            }
            if (key.Key == ConsoleKey.Escape)
            {
                return new QuitEvent();
            }
            if (key.Modifiers == ConsoleModifiers.None)
            {
                return new TextInputEvent(key.KeyChar);
            }

            return null;
        })
        .Where(e => e != null);  // Filter out null events
    }
}
