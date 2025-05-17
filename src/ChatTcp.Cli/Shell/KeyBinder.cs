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

            if (key.Key == ConsoleKey.Enter)
            {
                return new SendMessageEvent();
            }

            if (key.Key == ConsoleKey.Escape)
            {
                return new QuitEvent();
            }

            if ((key.Modifiers & (ConsoleModifiers.Control | ConsoleModifiers.Alt)) == (ConsoleModifiers.Control | ConsoleModifiers.Alt))
            {
                if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || char.IsSymbol(key.KeyChar))
                {
                    return new CharInputEvent(key.KeyChar);
                }
            }

            if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || char.IsSymbol(key.KeyChar) || key.KeyChar == ' ')
            {
                char c = key.KeyChar;
                if (key.Modifiers == ConsoleModifiers.Shift && char.IsLower(c))
                {
                    c = char.ToUpper(c);
                }

                return new CharInputEvent(c);
            }

            return null;
        })
        .Where(e => e != null);  // Filter out null events
    }
}
