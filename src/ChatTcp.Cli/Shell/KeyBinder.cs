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

            char c = key.KeyChar;
            if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ')
            {
                if (key.Modifiers == ConsoleModifiers.Shift && char.IsLower(c))
                {
                    c = char.ToUpper(c);
                }

                return new TextInputEvent(c);
            }

            return null;
        })
        .Where(e => e != null);  // Filter out null events
    }
}
